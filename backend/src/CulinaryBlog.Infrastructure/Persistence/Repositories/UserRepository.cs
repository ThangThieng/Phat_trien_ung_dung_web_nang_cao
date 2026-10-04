using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// <see cref="IUserRepository"/> trên <see cref="CulinaryBlogDbContext"/> (Buổi 3 — Dev 1). Chỉ đọc AspNetUsers; mọi
/// thao tác ghi người dùng đi qua UserManager (IIdentityService). Refresh token ghi qua đây rồi commit bằng UnitOfWork.
/// </summary>
public sealed class UserRepository(CulinaryBlogDbContext db) : IUserRepository
{
    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        await db.RefreshTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.SingleOrDefaultAsync(t => t.UserId == userId && t.TokenHash == tokenHash, cancellationToken);

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
}
