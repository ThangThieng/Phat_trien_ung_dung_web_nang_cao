using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository của module Auth: ĐỌC người dùng phục vụ nghiệp vụ + vòng đời refresh token ("phiên của người dùng").
/// Ghi người dùng (tạo, liên kết Google, khóa) luôn qua <c>IIdentityService</c> → <c>UserManager</c> vì UserManager còn
/// chuẩn hóa NormalizedEmail/UserName, đổi SecurityStamp, đếm lockout — ghi thẳng AspNetUsers sẽ bỏ sót.
/// Không kế thừa <see cref="IRepository{TEntity}"/>: ApplicationUser (Identity, Infrastructure) và RefreshToken (SRS §7.8)
/// đều không phải BaseEntity. Thay IRefreshTokenRepository của Buổi 2 (Buổi 3 — Dev 1).
/// </summary>
public interface IUserRepository
{
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>Refresh token (được EF theo dõi) của đúng <paramref name="userId"/> — dùng cho đăng xuất (FR-AUTH-005).</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Mọi UserName bắt đầu bằng <paramref name="prefix"/> (không phân biệt hoa/thường) — bộ sinh UserName chọn hậu tố bằng MỘT truy vấn.</summary>
    Task<IReadOnlyList<string>> GetUserNamesStartingWithAsync(string prefix, CancellationToken cancellationToken = default);
}
