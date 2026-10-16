using System.Globalization;

namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>
/// AUTH_ACCOUNT_LOCKED — tài khoản bị khóa tạm thời do đăng nhập sai quá số lần (FR-AUTH-002 A3, Identity Lockout).
/// Mang <c>lockoutEnd</c> và <c>retryAfterMinutes</c> trong extensions để Frontend hiển thị thời gian còn lại mà không
/// phải tách số từ câu thông báo tiếng Việt.
/// </summary>
public sealed class AccountLockedException : AuthDomainException
{
    public AccountLockedException(DateTimeOffset? lockoutEnd, int retryAfterMinutes)
        : base(ErrorCodes.AuthAccountLocked, BuildMessage(retryAfterMinutes))
    {
        LockoutEnd = lockoutEnd;
        RetryAfterMinutes = retryAfterMinutes;
        AddExtension("lockoutEnd", lockoutEnd);
        AddExtension("retryAfterMinutes", retryAfterMinutes);
    }

    public DateTimeOffset? LockoutEnd { get; }

    public int RetryAfterMinutes { get; }

    private static string BuildMessage(int retryAfterMinutes) =>
        "Tài khoản tạm thời bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau "
        + retryAfterMinutes.ToString(CultureInfo.InvariantCulture) + " phút.";
}
