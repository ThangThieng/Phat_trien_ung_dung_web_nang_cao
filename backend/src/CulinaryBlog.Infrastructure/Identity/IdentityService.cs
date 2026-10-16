using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Google.Apis.Auth;
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
    IOptions<GoogleAuthOptions> googleOptions,
    CulinaryBlogDbContext db,
    IRefreshTokenRepository refreshTokens) : IIdentityService
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeEmail(email);
        return userManager.Users.AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public async Task<IdentityUserInfo?> GetByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    public async Task<bool> IsLockedOutAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        return user is not null && await userManager.IsLockedOutAsync(user).ConfigureAwait(false);
    }

    public async Task<IdentityUserInfo> UpdateProfileAsync(string userId, ProfileUpdate update, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Người dùng không còn tồn tại.");
        if (update.HasDisplayName)
        {
            user.DisplayName = update.DisplayName!.Trim();
        }

        if (update.HasAvatarUrl)
        {
            user.AvatarUrl = update.AvatarUrl;
        }

        if (update.HasBio)
        {
            user.Bio = update.Bio;
        }

        ThrowIfFailed(await userManager.UpdateAsync(user).ConfigureAwait(false));
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    public async Task<PagedResult<UserAdminDto>> GetUsersAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().AsQueryable();
        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user => EF.Functions.ILike(user.Email!, $"%{term}%") || EF.Functions.ILike(user.DisplayName, $"%{term}%"));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var users = await query.OrderByDescending(user => user.CreatedAt).ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
        var ids = users.Select(user => user.Id).ToArray();
        var counts = await db.Recipes.Where(recipe => ids.Contains(recipe.AuthorId))
            .GroupBy(recipe => recipe.AuthorId).Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken).ConfigureAwait(false);
        var result = new List<UserAdminDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            result.Add(new UserAdminDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.AvatarUrl,
                [.. roles],
                user.IsActive,
                user.CreatedAt,
                counts.GetValueOrDefault(user.Id)));
        }

        return new PagedResult<UserAdminDto>(result, total, page, pageSize);
    }

    public async Task<IdentityUserInfo?> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        var user = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var target = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (target is null)
            {
                return null;
            }

            target.IsActive = isActive;
            ThrowIfFailed(await userManager.UpdateAsync(target).ConfigureAwait(false));
            if (!isActive)
            {
                await refreshTokens.RevokeAllForUserAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return target;
        }).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    public async Task<IdentityUserInfo> CreateUserAsync(
        string displayName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        var userName = await GenerateUniqueUserNameAsync(email, cancellationToken).ConfigureAwait(false);
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

    public async Task<IdentityUserInfo> AuthenticateGoogleAsync(string idToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(googleOptions.Value.ClientId))
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Dịch vụ đăng nhập Google chưa được cấu hình.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [googleOptions.Value.ClientId] })
                .ConfigureAwait(false);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException(ErrorCodes.AuthGoogleTokenInvalid, "Google ID token không hợp lệ hoặc đã hết hạn.");
        }
        catch (HttpRequestException)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không thể xác minh Google ID token lúc này.");
        }

        if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new BadRequestException(ErrorCodes.AuthGoogleTokenInvalid, "Google chưa xác minh địa chỉ email của tài khoản này.");
        }

        var user = await userManager.FindByLoginAsync("Google", payload.Subject).ConfigureAwait(false);
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(payload.Email).ConfigureAwait(false);
            if (user is null)
            {
                var userName = await GenerateUniqueUserNameAsync(payload.Email, cancellationToken).ConfigureAwait(false);
                user = ApplicationUser.Create(payload.Name ?? payload.Email, payload.Email, userName, timeProvider.GetUtcNow().UtcDateTime);
                user.AvatarUrl = payload.Picture;
                ThrowIfFailed(await userManager.CreateAsync(user).ConfigureAwait(false));
                ThrowIfFailed(await userManager.AddToRoleAsync(user, "Author").ConfigureAwait(false));
            }

            ThrowIfFailed(await userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google")).ConfigureAwait(false));
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return ToInfo(user, [.. roles]);
    }

    private static IdentityUserInfo ToInfo(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.AvatarUrl, user.Bio, user.CreatedAt, user.IsActive, roles);

    private async Task<string> GenerateUniqueUserNameAsync(string email, CancellationToken cancellationToken)
    {
        var prefix = email.Split('@')[0].Trim().ToLowerInvariant();
        const string allowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789-._@+";
        var baseName = new string(prefix.Where(character => allowedCharacters.Contains(character)).ToArray());
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "user";
        }

        baseName = baseName[..Math.Min(baseName.Length, 45)];

        var candidate = baseName;
        var suffix = 2;
        while (await UserNameExistsAsync(candidate, cancellationToken).ConfigureAwait(false))
        {
            candidate = $"{baseName}{suffix++}";
        }

        return candidate;
    }

    private Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var normalized = userManager.NormalizeName(userName);
        return userManager.Users.AnyAsync(u => u.NormalizedUserName == normalized, cancellationToken);
    }

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
