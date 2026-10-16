namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>AUTH_ACCOUNT_DISABLED — tài khoản bị Admin vô hiệu hóa (<c>IsActive = false</c>; FR-AUTH-002/003/004, FR-AUTH-008).</summary>
public sealed class AccountDisabledException : AuthDomainException
{
    public AccountDisabledException()
        : base(ErrorCodes.AuthAccountDisabled, "Tài khoản đã bị vô hiệu hóa bởi quản trị viên.")
    {
    }
}
