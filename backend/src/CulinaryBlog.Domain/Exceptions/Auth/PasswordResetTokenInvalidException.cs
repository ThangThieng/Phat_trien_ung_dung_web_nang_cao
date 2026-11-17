namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>FR-AUTH-011: reset-password link is invalid, expired, or has already been consumed.</summary>
public sealed class PasswordResetTokenInvalidException : AuthDomainException
{
    public PasswordResetTokenInvalidException()
        : base(ErrorCodes.AuthPasswordResetTokenInvalid, "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.")
    {
    }
}
