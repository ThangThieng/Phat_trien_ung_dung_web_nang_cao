using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository của module Auth: ĐỌC người dùng phục vụ nghiệp vụ + vòng đời refresh token ("phiên của người dùng").
/// Ghi người dùng (tạo, liên kết Google, khóa) luôn qua <c>IIdentityService</c> → <c>UserManager</c> vì UserManager còn
/// chuẩn hóa NormalizedEmail/UserName, đổi SecurityStamp, đếm lockout — ghi thẳng AspNetUsers sẽ bỏ sót.
/// Không kế thừa <see cref="IRepository{TEntity}"/>: ApplicationUser (Identity, Infrastructure) và RefreshToken (SRS §7.8)
/// đều không phải BaseEntity. Thay IRefreshTokenRepository của Buổi 2 (Buổi 3 — Dev 1); Buổi 4 bổ sung các thao tác
/// của refresh rotation, khóa tài khoản và danh sách phiên — một repository cho cả bốn nhu cầu.
/// </summary>
public interface IUserRepository
{
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>Refresh token (được EF theo dõi) của đúng <paramref name="userId"/> — dùng cho đăng xuất (FR-AUTH-005).</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// FR-AUTH-004: tra theo hash KHÔNG kèm UserId — lúc refresh, access token đã hết hạn nên chưa biết người gọi là ai.
    /// Không theo dõi (AsNoTracking): mọi thay đổi trạng thái token đi qua các phương thức cập nhật có điều kiện bên dưới.
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thu hồi token NẾU nó còn chưa bị thu hồi — một câu UPDATE có điều kiện <c>RevokedAt IS NULL</c>, nên hai request
    /// refresh cùng một token chạy đồng thời chỉ một request thắng (không rẽ nhánh token family).
    /// </summary>
    /// <returns><c>true</c> nếu chính lời gọi này đã thu hồi token.</returns>
    Task<bool> RevokeRefreshTokenIfActiveAsync(Guid tokenId, DateTime utcNow, string? replacedByTokenHash, CancellationToken cancellationToken = default);

    /// <summary>Thu hồi MỌI refresh token còn hiệu lực của người dùng (reuse detection, khóa tài khoản, đăng xuất mọi thiết bị).</summary>
    /// <returns>Số token vừa bị thu hồi.</returns>
    Task<int> RevokeAllRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default);

    /// <summary>FR-AUTH-009: phiên còn hiệu lực (<c>RevokedAt IS NULL AND ExpiresAt &gt; now</c>), mới nhất trước.</summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default);

    /// <summary>FR-AUTH-009: token theo id VÀ chủ sở hữu — phiên của người khác trả null (endpoint trả 404, không lộ id).</summary>
    Task<RefreshToken?> GetRefreshTokenByIdAsync(string userId, Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>Mọi UserName bắt đầu bằng <paramref name="prefix"/> (không phân biệt hoa/thường) — bộ sinh UserName chọn hậu tố bằng MỘT truy vấn.</summary>
    Task<IReadOnlyList<string>> GetUserNamesStartingWithAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>FR-AUTH-008: danh sách người dùng cho Admin (lọc email/displayName, IsActive), mới tạo trước.</summary>
    Task<PagedResult<UserAdminDto>> GetUsersPageAsync(UserListCriteria criteria, CancellationToken cancellationToken = default);
}
