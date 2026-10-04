namespace CulinaryBlog.Application.Features.Auth;

/// <summary>FR-AUTH-001 bước 12 – AuthResponseDto.</summary>
public sealed record AuthResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, int ExpiresIn, UserDto User);

/// <summary>SRS §8.1 – <c>{ id, email, displayName, avatarUrl, bio, roles }</c> (D-1, MT-12). Không có UserName/PasswordHash/SecurityStamp.</summary>
public sealed record UserDto(string Id, string Email, string DisplayName, string? AvatarUrl, string? Bio, IReadOnlyList<string> Roles);

/// <summary>Thông tin user trả về từ tầng Identity (Infrastructure) – không bao giờ chứa PasswordHash/SecurityStamp.</summary>
public sealed record IdentityUserInfo(
    string Id,
    string DisplayName,
    string Email,
    string UserName,
    string? AvatarUrl,
    bool IsActive,
    IReadOnlyList<string> Roles,
    string? Bio = null)
{
    public UserDto ToDto() => new(Id, Email, DisplayName, AvatarUrl, Bio, Roles);
}

public enum PasswordCheckStatus
{
    Success,
    InvalidCredentials,
    LockedOut,
    Disabled,
}

public sealed record PasswordCheckResult(PasswordCheckStatus Status, IdentityUserInfo? User, DateTimeOffset? LockoutEnd);

/// <summary>
/// Bọc ASP.NET Core Identity (UserManager) – PBKDF2 hashing, lockout – để Application không phụ thuộc Identity EF.
/// Mọi thao tác GHI người dùng đi qua đây (xem giải trình ở <c>IUserRepository</c>).
/// </summary>
public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>Tạo user (UserManager.CreateAsync – hash PBKDF2), UserName do <see cref="IUserNameGenerator"/> sinh (D-1), gán role.</summary>
    Task<IdentityUserInfo> CreateUserAsync(string displayName, string email, string password, string role, CancellationToken cancellationToken);

    /// <summary>
    /// FR-AUTH-003 bước liên kết tài khoản: đã liên kết Google → trả user; email đã có → chỉ liên kết khi Google xác nhận
    /// <c>email_verified</c> (ngược lại ném BusinessRuleViolationException 400); hoàn toàn mới → tạo user role Author.
    /// </summary>
    Task<IdentityUserInfo> FindOrCreateGoogleUserAsync(GoogleIdTokenPayload payload, CancellationToken cancellationToken);

    /// <summary>Xác minh mật khẩu có xử lý lockout: sai 5 lần → khóa 15 phút (FR-AUTH-002 A3).</summary>
    Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);
}

/// <summary>Dữ liệu đã được Google ký mà hệ thống cần từ ID Token (FR-AUTH-003).</summary>
public sealed record GoogleIdTokenPayload(string Subject, string? Email, bool EmailVerified, string? Name, string? Picture);

/// <summary>
/// Xác minh Google ID Token (chữ ký, iss, aud, exp). Interface ở Application để handler không tham chiếu Google.Apis.Auth
/// và để integration test thay bằng bản giả — không gọi Google thật. Cài đặt dịch lỗi thư viện sang lỗi nghiệp vụ:
/// chữ ký/aud/exp sai → InvalidTokenException.GoogleTokenRejected() (401); không lấy được JWKS → BadGatewayException (502).
/// </summary>
public interface IGoogleIdTokenValidator
{
    Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken);
}

/// <summary>
/// D-1: sinh UserName từ phần trước "@" của email (chữ thường, chỉ giữ ký tự Identity cho phép), trùng thì thêm hậu tố
/// 2, 3… — tra bằng MỘT truy vấn <c>GetUserNamesStartingWithAsync</c>. Người dùng không bao giờ nhập UserName.
/// </summary>
public interface IUserNameGenerator
{
    Task<string> GenerateAsync(string email, CancellationToken cancellationToken);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt, int ExpiresInSeconds);

public sealed record GeneratedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAt);

/// <summary>Sinh JWT access token (HS256, 15 phút) và refresh token (7 ngày, DB chỉ lưu SHA-256).</summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(IdentityUserInfo user);

    GeneratedRefreshToken CreateRefreshToken();

    string HashToken(string rawToken);
}

/// <summary>FR-JOB-001 – đẩy job gửi email chào mừng vào Hangfire (fire-and-forget).</summary>
public interface IWelcomeEmailScheduler
{
    void ScheduleWelcomeEmail(string userId);
}
