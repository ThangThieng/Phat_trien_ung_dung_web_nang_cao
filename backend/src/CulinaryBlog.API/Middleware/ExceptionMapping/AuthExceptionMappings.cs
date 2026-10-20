using CulinaryBlog.Domain.Exceptions.Auth;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

/// <summary>
/// Ánh xạ lỗi nghiệp vụ module Auth sang mã HTTP (SRS §3.1, Phụ lục B). <c>AccountLockedException</c> giữ 423 (MT-08 chỉ
/// loại 422); 403 thuộc về tài khoản bị vô hiệu hóa. Được <c>ExceptionStatusMap.FromAssembly</c> quét tự động.
/// </summary>
public sealed class AuthExceptionMappings : IExceptionStatusMapping
{
    public void Configure(ExceptionStatusMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        map.Map<InvalidCredentialsException>(StatusCodes.Status401Unauthorized)
            .Map<InvalidTokenException>(StatusCodes.Status401Unauthorized)
            .Map<AccountLockedException>(StatusCodes.Status423Locked)
            .Map<AccountDisabledException>(StatusCodes.Status403Forbidden)
            .Map<EmailAlreadyExistsException>(StatusCodes.Status409Conflict)
            .Map<UserNotFoundException>(StatusCodes.Status404NotFound);
    }
}
