using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-001 bước 12 – AuthResponseDto.</summary>
public sealed record AuthResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, int ExpiresIn, UserDto User);

public sealed record UserDto(string Id, string DisplayName, string Email, string? AvatarUrl, IReadOnlyList<string> Roles);

/// <summary>Thông tin user trả về từ tầng Identity (Infrastructure) – không bao giờ chứa PasswordHash/SecurityStamp.</summary>
public sealed record IdentityUserInfo(
    string Id,
    string DisplayName,
    string Email,
    string UserName,
    string? AvatarUrl,
    bool IsActive,
    IReadOnlyList<string> Roles)
{
    public UserDto ToDto() => new(Id, DisplayName, Email, AvatarUrl, Roles);
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

    /// <summary>Tạo user (UserManager.CreateAsync – hash PBKDF2) và gán role. Ném ValidationException nếu Identity từ chối.</summary>
    Task<IdentityUserInfo> CreateUserAsync(string displayName, string email, string password, string role, CancellationToken cancellationToken);

    Task<IdentityUserInfo> AuthenticateGoogleAsync(string idToken, CancellationToken cancellationToken);

    /// <summary>Xác minh mật khẩu có xử lý lockout: sai 5 lần → khóa 15 phút (FR-AUTH-002 A3).</summary>
    Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt, int ExpiresInSeconds);

public sealed record GeneratedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAt);

/// <summary>Sinh JWT access token (HS256, 15 phút) và refresh token (512-bit, 7 ngày, lưu SHA-256).</summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(IdentityUserInfo user);

    GeneratedRefreshToken CreateRefreshToken();

    string HashToken(string rawToken);
}

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task RevokeAsync(RefreshToken token, DateTime revokedAt, CancellationToken cancellationToken);
}

/// <summary>FR-JOB-001 – đẩy job gửi email chào mừng vào Hangfire (fire-and-forget).</summary>
public interface IWelcomeEmailScheduler
{
    void ScheduleWelcomeEmail(string userId);
}
