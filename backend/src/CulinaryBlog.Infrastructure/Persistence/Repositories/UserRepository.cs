using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// <see cref="IUserRepository"/> trên <see cref="CulinaryBlogDbContext"/> (Buổi 3–4 — Dev 1). Chỉ ĐỌC AspNetUsers; mọi
/// thao tác ghi người dùng đi qua UserManager (IIdentityService). Refresh token ghi qua đây và commit bằng UnitOfWork;
/// các lệnh thu hồi hàng loạt là UPDATE có điều kiện chạy ngay (ExecuteUpdate) — nằm trong transaction nếu người gọi mở.
/// </summary>
public sealed class UserRepository(CulinaryBlogDbContext db) : IUserRepository
{
    /// <summary>Ký tự đặc biệt của ILIKE được escape bằng "\" (tham số thứ ba của EF.Functions.ILike).</summary>
    private const string LikeEscape = "\\";

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        await db.RefreshTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.SingleOrDefaultAsync(t => t.UserId == userId && t.TokenHash == tokenHash, cancellationToken);

    /// <summary>TokenHash có UNIQUE index (IDX_RefreshToken_Hash) → tra cứu một dòng theo index.</summary>
    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task<bool> RevokeRefreshTokenIfActiveAsync(
        Guid tokenId,
        DateTime utcNow,
        string? replacedByTokenHash,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.RefreshTokens
            .Where(t => t.Id == tokenId && t.RevokedAt == null)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(t => t.RevokedAt, utcNow)
                    .SetProperty(t => t.ReplacedByTokenHash, replacedByTokenHash),
                cancellationToken)
            .ConfigureAwait(false);
        return rows == 1;
    }

    public Task<int> RevokeAllRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default) =>
        db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.RevokedAt, utcNow), cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default) =>
        await db.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > utcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<RefreshToken?> GetRefreshTokenByIdAsync(string userId, Guid tokenId, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.SingleOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<string>> GetUserNamesStartingWithAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        // NormalizedUserName có unique index (UserNameIndex) → LIKE 'PREFIX%' dùng được index, không quét bảng.
        var normalized = prefix.ToUpperInvariant();
        return await db.Users
            .AsNoTracking()
            .Where(u => u.NormalizedUserName != null && u.NormalizedUserName.StartsWith(normalized))
            .Select(u => u.UserName!)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// FR-AUTH-008: một truy vấn đếm + một truy vấn trang; roles và recipeCount là subquery trong projection (không N+1).
    /// recipeCount chỉ đếm công thức chưa xóa mềm (Global Query Filter), mọi trạng thái.
    /// </summary>
    public async Task<PagedResult<UserAdminDto>> GetUsersPageAsync(UserListCriteria criteria, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = db.Users.AsNoTracking();

        if (criteria.Search is { } search)
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Email!, pattern, LikeEscape) || EF.Functions.ILike(u.DisplayName, pattern, LikeEscape));
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(u => u.IsActive == isActive);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(u => new UserAdminDto(
                u.Id,
                u.Email ?? string.Empty,
                u.DisplayName,
                u.AvatarUrl,
                db.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
                    .OrderBy(name => name)
                    .ToList(),
                u.IsActive,
                u.CreatedAt,
                db.Recipes.Count(r => r.AuthorId == u.Id)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<UserAdminDto>(items, total, criteria.Page, criteria.PageSize);
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
            .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
            .Replace("_", LikeEscape + "_", StringComparison.Ordinal);
}
