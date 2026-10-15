using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Exceptions.Auth;

public abstract class AuthDomainException(string code, string message) : DomainException(message)
{
    public string Code { get; } = code;
}

public sealed class InvalidCredentialsException()
    : AuthDomainException("AUTH_INVALID_CREDENTIALS", "Email hoặc mật khẩu không đúng.");

public sealed class InvalidTokenException(string message = "Token không hợp lệ.", string code = "AUTH_TOKEN_INVALID")
    : AuthDomainException(code, message);

public sealed class AccountDisabledException()
    : AuthDomainException("AUTH_ACCOUNT_DISABLED", "Tài khoản đã bị vô hiệu hóa bởi quản trị viên.");

public sealed class UserNotFoundException()
    : AuthDomainException("AUTH_USER_NOT_FOUND", "Không tìm thấy người dùng.");

public sealed class EmailAlreadyExistsException()
    : AuthDomainException("AUTH_EMAIL_EXISTS", "Email đã được đăng ký bởi tài khoản khác.");

public sealed class AccountLockedException(DateTimeOffset? lockoutEnd, int retryAfterMinutes)
    : AuthDomainException("AUTH_ACCOUNT_LOCKED", $"Tài khoản tạm thời bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau {retryAfterMinutes} phút.")
{
    public DateTimeOffset? LockoutEnd { get; } = lockoutEnd;

    public int RetryAfterMinutes { get; } = retryAfterMinutes;
}
