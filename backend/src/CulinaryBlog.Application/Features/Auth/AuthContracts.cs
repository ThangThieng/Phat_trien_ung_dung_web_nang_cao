using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-001 bước 12 – AuthResponseDto.</summary>
public sealed record AuthResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, int ExpiresIn, UserDto User);

public sealed record UserDto(string Id, string DisplayName, string Email, string? AvatarUrl, string? Bio, IReadOnlyList<string> Roles);

public sealed record UserProfileDto(string Id, string DisplayName, string Email, string? AvatarUrl, string? Bio, IReadOnlyList<string> Roles, DateTime CreatedAt);

public sealed record UserAdminDto(string Id, string Email, string DisplayName, string? AvatarUrl, IReadOnlyList<string> Roles, bool IsActive, DateTime CreatedAt, int RecipeCount);

public sealed record SessionDto(Guid Id, DateTime CreatedAt, string? CreatedByIp, DateTime ExpiresAt, bool IsCurrent);

public sealed record ProfileUpdate(string? DisplayName, bool HasDisplayName, string? AvatarUrl, bool HasAvatarUrl, string? Bio, bool HasBio);

/// <summary>Thông tin user trả về từ tầng Identity (Infrastructure) – không bao giờ chứa PasswordHash/SecurityStamp.</summary>
public sealed record IdentityUserInfo(
    string Id,
    string DisplayName,
    string Email,
    string UserName,
    string? AvatarUrl,
    string? Bio,
    DateTime CreatedAt,
    bool IsActive,
    IReadOnlyList<string> Roles)
{
    public UserDto ToDto() => new(Id, DisplayName, Email, AvatarUrl, Bio, Roles);

    public UserProfileDto ToProfileDto() => new(Id, DisplayName, Email, AvatarUrl, Bio, Roles, CreatedAt);
}

public enum PasswordCheckStatus
{
    Success,
    InvalidCredentials,
    LockedOut,
    Disabled,
}

public sealed record PasswordCheckResult(PasswordCheckStatus Status, IdentityUserInfo? User, DateTimeOffset? LockoutEnd);

/// <summary>Bọc ASP.NET Core Identity (UserManager) – PBKDF2 hashing, lockout – để Application không phụ thuộc Identity EF.</summary>
public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<IdentityUserInfo?> GetByIdAsync(string userId, CancellationToken cancellationToken);

    Task<bool> IsLockedOutAsync(string userId, CancellationToken cancellationToken);

    Task<IdentityUserInfo> UpdateProfileAsync(string userId, ProfileUpdate update, CancellationToken cancellationToken);

    Task<PagedResult<UserAdminDto>> GetUsersAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken cancellationToken);

    Task<IdentityUserInfo?> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken);

    /// <summary>Tạo user (UserManager.CreateAsync – hash PBKDF2) và gán role. Ném ValidationException nếu Identity từ chối.</summary>
    Task<IdentityUserInfo> CreateUserAsync(string displayName, string email, string password, string role, CancellationToken cancellationToken);

    Task<IdentityUserInfo> AuthenticateGoogleAsync(string idToken, CancellationToken cancellationToken);

    /// <summary>Xác minh mật khẩu có xử lý lockout: sai 5 lần → khóa 15 phút (FR-AUTH-002 A3).</summary>
    Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt, int ExpiresInSeconds);

public sealed record GeneratedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAt);

/// <summary>Sinh JWT access token (HS256, 15 phút) và refresh token (256-bit, 7 ngày, lưu SHA-256).</summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(IdentityUserInfo user, Guid sessionId);

    GeneratedRefreshToken CreateRefreshToken();

    string HashToken(string rawToken);
}

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task RevokeAsync(RefreshToken token, DateTime revokedAt, CancellationToken cancellationToken);

    Task RevokeAllForUserAsync(string userId, DateTime revokedAt, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(string userId, DateTime now, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken);
}

/// <summary>FR-JOB-001 – đẩy job gửi email chào mừng vào Hangfire (fire-and-forget).</summary>
public interface IWelcomeEmailScheduler
{
    void ScheduleWelcomeEmail(string userId);
}
