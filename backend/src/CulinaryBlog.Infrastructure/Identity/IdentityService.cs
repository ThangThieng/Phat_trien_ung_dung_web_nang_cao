using CulinaryBlog.Application.Features.Auth;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// IIdentityService dựa trên ASP.NET Core Identity UserManager:
/// hash mật khẩu PBKDF2 (IPasswordHasher mặc định của Identity – cấu hình 100.000 vòng lặp), lockout 5 lần / 15 phút.
/// </summary>
public sealed class IdentityService(UserManager<ApplicationUser> userManager, TimeProvider timeProvider) : IIdentityService
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeEmail(email);
        return userManager.Users.AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeName(userName);
        return userManager.Users.AnyAsync(u => u.NormalizedUserName == normalized, cancellationToken);
    }

    public async Task<IdentityUserInfo> CreateUserAsync(
        string fullName,
        string email,
        string userName,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        var user = ApplicationUser.Create(fullName, email, userName, timeProvider.GetUtcNow().UtcDateTime);

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

    private static IdentityUserInfo ToInfo(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.AvatarUrl, user.IsActive, roles);

    /// <summary>Chuyển IdentityError thành FluentValidation failures → HTTP 400 VALIDATION_ERROR (FR-AUTH-001 A2, MT-08).</summary>
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
