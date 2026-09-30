namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>
/// Token không dùng được để xác thực (401). Một lớp, nhiều mã Phụ lục B — mỗi tình huống một factory để nơi ném
/// không phải tự chọn mã: <see cref="GoogleTokenRejected"/> (FR-AUTH-003 A1).
/// Mã HTTP gán theo KIỂU (401), nên thêm factory mới không phải sửa middleware.
/// </summary>
public sealed class InvalidTokenException : AuthDomainException
{
    public InvalidTokenException()
        : this(ErrorCodes.AuthTokenInvalid, "Token không hợp lệ.")
    {
    }

    private InvalidTokenException(string code, string message, Exception? innerException = null)
        : base(code, message, innerException)
    {
    }

    /// <summary>AUTH_GOOGLE_TOKEN_INVALID — chữ ký, <c>aud</c> hoặc <c>exp</c> của Google ID Token không hợp lệ (FR-AUTH-003 A1, MT-61).</summary>
    public static InvalidTokenException GoogleTokenRejected(Exception? innerException = null) =>
        new(ErrorCodes.AuthGoogleTokenInvalid, "Google ID token không hợp lệ hoặc đã hết hạn.", innerException);

    /// <summary>AUTH_REFRESH_TOKEN_EXPIRED — refresh token quá 7 ngày (FR-AUTH-004 A2); client phải đăng nhập lại.</summary>
    public static InvalidTokenException RefreshTokenExpired() =>
        new(ErrorCodes.AuthRefreshTokenExpired, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");

    /// <summary>AUTH_REFRESH_TOKEN_REVOKED — refresh token đã bị thu hồi mà vẫn được dùng lại: reuse detection (FR-AUTH-004 A3).</summary>
    public static InvalidTokenException RefreshTokenRevoked() =>
        new(ErrorCodes.AuthRefreshTokenRevoked, "Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại.");
}
