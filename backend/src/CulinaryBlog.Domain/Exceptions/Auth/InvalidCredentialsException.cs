namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>
/// AUTH_INVALID_CREDENTIALS — email hoặc mật khẩu sai (FR-AUTH-002 A1). Một thông điệp chung cho cả hai trường hợp
/// để không tiết lộ email có tồn tại hay không (chống user enumeration).
/// </summary>
public sealed class InvalidCredentialsException : AuthDomainException
{
    public InvalidCredentialsException()
        : base(ErrorCodes.AuthInvalidCredentials, "Email hoặc mật khẩu không đúng.")
    {
    }
}
