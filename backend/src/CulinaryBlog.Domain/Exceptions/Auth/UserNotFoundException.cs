namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>AUTH_USER_NOT_FOUND — người dùng đích không tồn tại (FR-AUTH-008 A3; CR-2026-04 / MT-59).</summary>
public sealed class UserNotFoundException : AuthDomainException
{
    public UserNotFoundException(string userId)
        : base(ErrorCodes.AuthUserNotFound, $"Không tìm thấy người dùng '{userId}'.")
    {
        UserId = userId;
    }

    public string UserId { get; }
}
