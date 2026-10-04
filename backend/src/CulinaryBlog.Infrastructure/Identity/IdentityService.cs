using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// IIdentityService dựa trên ASP.NET Core Identity UserManager:
/// hash mật khẩu PBKDF2 (IPasswordHasher mặc định của Identity – cấu hình 100.000 vòng lặp), lockout 5 lần / 15 phút.
/// Mọi thao tác GHI người dùng của hệ thống đi qua lớp này.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserNameGenerator userNameGenerator,
    TimeProvider timeProvider) : IIdentityService
{
    private const string GoogleProvider = "Google";

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeEmail(email);
        return userManager.Users.AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public async Task<IdentityUserInfo> CreateUserAsync(
        string displayName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        var userName = await userNameGenerator.GenerateAsync(email, cancellationToken).ConfigureAwait(false);
        var user = ApplicationUser.Create(displayName, email, userName, timeProvider.GetUtcNow().UtcDateTime);

        // UserManager.CreateAsync → IPasswordHasher<ApplicationUser> (PBKDF2-HMAC-SHA512)
        ThrowIfFailed(await userManager.CreateAsync(user, password).ConfigureAwait(false));
        ThrowIfFailed(await userManager.AddToRoleAsync(user, role).ConfigureAwait(false));

        return ToInfo(user, [role]);
    }

    public async Task<IdentityUserInfo> FindOrCreateGoogleUserAsync(GoogleIdTokenPayload payload, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var email = payload.Email ?? throw new ArgumentException("Payload Google phải có email.", nameof(payload));

        // 1. Đã liên kết Google từ trước → đăng nhập.
        var user = await userManager.FindByLoginAsync(GoogleProvider, payload.Subject).ConfigureAwait(false);
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user is null)
            {
                // 3. Hoàn toàn mới → tạo tài khoản Author (không mật khẩu; chỉ đăng nhập được bằng Google).
                var userName = await userNameGenerator.GenerateAsync(email, cancellationToken).ConfigureAwait(false);
                user = ApplicationUser.Create(
                    string.IsNullOrWhiteSpace(payload.Name) ? userName : payload.Name.Trim(),
                    email,
                    userName,
                    timeProvider.GetUtcNow().UtcDateTime);
                user.AvatarUrl = payload.Picture;
                user.EmailConfirmed = payload.EmailVerified;
                ThrowIfFailed(await userManager.CreateAsync(user).ConfigureAwait(false));
                ThrowIfFailed(await userManager.AddToRoleAsync(user, Roles.Author).ConfigureAwait(false));
            }
            else if (!payload.EmailVerified)
            {
                // 2. Email đã có tài khoản: chỉ liên kết khi Google xác nhận email — nếu không, kẻ tấn công tạo tài khoản
                //    Google gắn email của nạn nhân là chiếm được tài khoản Culinary Blog của nạn nhân.
                throw new BusinessRuleViolationException(
                    ErrorCodes.AuthGoogleTokenInvalid,
                    "Google chưa xác minh địa chỉ email này nên không thể liên kết với tài khoản đang có.");
            }

            ThrowIfFailed(await userManager.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, payload.Subject, GoogleProvider)).ConfigureAwait(false));
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    public async Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null)
        {
            return new PasswordCheckResult(PasswordCheckStatus.InvalidCredentials, null, null);
        }

        if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
        {
            return new PasswordCheckResult(PasswordCheckStatus.LockedOut, null, user.LockoutEnd);
        }

        if (!await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false))
        {
            // A3: tăng AccessFailedCount – đạt 5 lần thì Identity tự đặt LockoutEnd = now + 15 phút
            await userManager.AccessFailedAsync(user).ConfigureAwait(false);

            return await userManager.IsLockedOutAsync(user).ConfigureAwait(false)
                ? new PasswordCheckResult(PasswordCheckStatus.LockedOut, null, user.LockoutEnd)
                : new PasswordCheckResult(PasswordCheckStatus.InvalidCredentials, null, null);
        }

        // Chỉ báo "bị vô hiệu hóa" khi mật khẩu đúng – tránh lộ thông tin tài khoản
        if (!user.IsActive)
        {
            return new PasswordCheckResult(PasswordCheckStatus.Disabled, null, null);
        }

        await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new PasswordCheckResult(PasswordCheckStatus.Success, ToInfo(user, [.. roles]), null);
    }

    internal static IdentityUserInfo ToInfo(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.AvatarUrl, user.IsActive, roles, user.Bio);

    /// <summary>Chuyển IdentityError thành FluentValidation failures → HTTP 400 VALIDATION_ERROR (FR-AUTH-001 A2, MT-08).</summary>
    internal static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var failures = result.Errors.Select(e => new ValidationFailure(MapProperty(e.Code), e.Description) { ErrorCode = e.Code });
        throw new ValidationException(failures);
    }

    private static string MapProperty(string identityErrorCode) => identityErrorCode switch
    {
        _ when identityErrorCode.StartsWith("Password", StringComparison.Ordinal) => "password",
        _ when identityErrorCode.Contains("Email", StringComparison.Ordinal) => "email",
        _ when identityErrorCode.Contains("UserName", StringComparison.Ordinal) => "userName",
        _ => string.Empty,
    };
}
