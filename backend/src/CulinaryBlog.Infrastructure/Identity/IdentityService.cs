using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions.Auth;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// IIdentityService dựa trên ASP.NET Core Identity UserManager:
/// hash mật khẩu PBKDF2 (IPasswordHasher mặc định của Identity – cấu hình 100.000 vòng lặp), lockout 5 lần / 15 phút.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider,
    IUserNameGenerator userNameGenerator) : IIdentityService
{
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
        var created = await userManager.CreateAsync(user, password).ConfigureAwait(false);
        ThrowIfFailed(created);

        var addedRole = await userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
        ThrowIfFailed(addedRole);

        return ToInfo(user, [role]);
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

    public async Task<IdentityUserInfo> AuthenticateGoogleAsync(VerifiedGoogleIdentity googleIdentity, CancellationToken cancellationToken)
    {
        if (!googleIdentity.EmailVerified || string.IsNullOrWhiteSpace(googleIdentity.Email) || string.IsNullOrWhiteSpace(googleIdentity.Subject))
        {
            throw new BadRequestException(ErrorCodes.AuthGoogleTokenInvalid, "Google chưa xác minh địa chỉ email của tài khoản này.");
        }

        var user = await userManager.FindByLoginAsync("Google", googleIdentity.Subject).ConfigureAwait(false);
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(googleIdentity.Email).ConfigureAwait(false);
            if (user is not null && !user.IsActive)
            {
                throw new AccountDisabledException();
            }

            if (user is null)
            {
                var userName = await userNameGenerator.GenerateAsync(googleIdentity.Email, cancellationToken).ConfigureAwait(false);
                user = ApplicationUser.Create(googleIdentity.Name ?? googleIdentity.Email, googleIdentity.Email, userName, timeProvider.GetUtcNow().UtcDateTime);
                user.AvatarUrl = googleIdentity.Picture;
                ThrowIfFailed(await userManager.CreateAsync(user).ConfigureAwait(false));
                ThrowIfFailed(await userManager.AddToRoleAsync(user, "Author").ConfigureAwait(false));
            }

            ThrowIfFailed(await userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleIdentity.Subject, "Google")).ConfigureAwait(false));
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    private static IdentityUserInfo ToInfo(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.AvatarUrl, user.Bio, user.IsActive, roles);

    /// <summary>Chuyển IdentityError thành FluentValidation failures → HTTP 422 (FR-AUTH-001 A2).</summary>
    private static void ThrowIfFailed(IdentityResult result)
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
