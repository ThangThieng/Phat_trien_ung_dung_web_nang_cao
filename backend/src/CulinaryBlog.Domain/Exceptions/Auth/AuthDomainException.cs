using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Exceptions.Auth;

public abstract class AuthDomainException(string code, string message) : DomainException(message)
{
    public string Code { get; } = code;
}

public sealed class InvalidTokenException : AuthDomainException
{
    public InvalidTokenException(string message = "Token không hợp lệ.")
        : this("AUTH_TOKEN_INVALID", message)
    {
    }

    private InvalidTokenException(string code, string message)
        : base(code, message)
    {
    }

    public static InvalidTokenException RefreshTokenExpired() => new("AUTH_REFRESH_TOKEN_EXPIRED", "Refresh token đã hết hạn.");

    public static InvalidTokenException RefreshTokenRevoked() => new("AUTH_REFRESH_TOKEN_REVOKED", "Refresh token đã bị thu hồi; vui lòng đăng nhập lại.");
}

public sealed class AccountDisabledException() : AuthDomainException("AUTH_ACCOUNT_DISABLED", "Tài khoản đã bị vô hiệu hóa.");

public sealed class UserNotFoundException() : AuthDomainException("AUTH_USER_NOT_FOUND", "Không tìm thấy người dùng.");
