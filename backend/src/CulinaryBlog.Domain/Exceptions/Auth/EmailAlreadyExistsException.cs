namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>AUTH_EMAIL_EXISTS — email đã được đăng ký bởi tài khoản khác (FR-AUTH-001 A1).</summary>
public sealed class EmailAlreadyExistsException : AuthDomainException
{
    public EmailAlreadyExistsException()
        : base(ErrorCodes.AuthEmailExists, "Email đã được đăng ký bởi tài khoản khác.")
    {
    }
}
