using System.Globalization;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// D-1 (MT-46): UserName do hệ thống sinh — người dùng không nhập, không thấy ở đâu. Lấy phần trước "@" của email,
/// chữ thường, chỉ giữ ký tự nằm trong <see cref="UserOptions.AllowedUserNameCharacters"/> (cùng cấu hình UserManager
/// dùng để kiểm tra, nên tên sinh ra không bao giờ bị Identity từ chối). Trùng thì thêm hậu tố 2, 3…:
/// <c>an.nguyen@x.com</c> rồi <c>an.nguyen@y.com</c> → <c>an.nguyen</c>, <c>an.nguyen2</c>.
/// </summary>
public sealed class UserNameGenerator(IUnitOfWork unitOfWork, IOptions<IdentityOptions> identityOptions) : IUserNameGenerator
{
    /// <summary>Chừa chỗ cho hậu tố số trong giới hạn 256 của cột UserName; tên dài hơn không mang thêm ý nghĩa gì.</summary>
    private const int MaxBaseLength = 50;

    private const string FallbackBase = "user";

    public async Task<string> GenerateAsync(string email, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var allowed = identityOptions.Value.User.AllowedUserNameCharacters;
        var localPart = email.Split('@')[0].Trim().ToLowerInvariant();
        var baseName = new string(localPart.Where(c => string.IsNullOrEmpty(allowed) || allowed.Contains(c, StringComparison.Ordinal)).ToArray());
        if (baseName.Length == 0)
        {
            baseName = FallbackBase;
        }

        baseName = baseName[..Math.Min(baseName.Length, MaxBaseLength)];

        // MỘT truy vấn lấy mọi tên cùng tiền tố, rồi chọn hậu tố trong bộ nhớ — thay vì thử từng tên một.
        var taken = (await unitOfWork.Users.GetUserNamesStartingWithAsync(baseName, cancellationToken).ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!taken.Contains(baseName))
        {
            return baseName;
        }

        // Có N tên cùng tiền tố thì trong các hậu tố 2..N+1 chắc chắn còn ít nhất một số trống.
        var suffix = 2;
        while (taken.Contains(baseName + suffix.ToString(CultureInfo.InvariantCulture)))
        {
            suffix++;
        }

        return baseName + suffix.ToString(CultureInfo.InvariantCulture);
    }
}
