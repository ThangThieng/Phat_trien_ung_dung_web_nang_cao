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
}
