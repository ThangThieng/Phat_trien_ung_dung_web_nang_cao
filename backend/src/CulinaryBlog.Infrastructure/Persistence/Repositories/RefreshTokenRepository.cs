using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(CulinaryBlogDbContext db) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken) =>
        await db.RefreshTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public async Task<bool> TryRotateAsync(RefreshToken token, RefreshToken replacement, DateTime now, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var consumed = await db.RefreshTokens
                .Where(current => current.Id == token.Id && current.RevokedAt == null && current.ExpiresAt > now)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(current => current.RevokedAt, now)
                        .SetProperty(current => current.ReplacedByTokenHash, replacement.TokenHash),
                    cancellationToken).ConfigureAwait(false);
            if (consumed == 0)
            {
                return false;
            }

            try
            {
                await db.RefreshTokens.AddAsync(replacement, cancellationToken).ConfigureAwait(false);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                // A rollback must not leave an added replacement in the tracker on an execution-strategy retry.
                db.Entry(replacement).State = EntityState.Detached;
                throw;
            }
        }).ConfigureAwait(false);
    }

    public Task RevokeAsync(RefreshToken token, DateTime revokedAt, CancellationToken cancellationToken)
    {
        token.Revoke(revokedAt);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(string userId, DateTime revokedAt, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens.Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var token in tokens)
        {
            token.Revoke(revokedAt);
        }
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(string userId, DateTime now, CancellationToken cancellationToken) =>
        await db.RefreshTokens.Where(token => token.UserId == userId && token.RevokedAt == null && token.ExpiresAt > now)
            .OrderByDescending(token => token.CreatedAt).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task RevokeFamilyAsync(string userId, string startingTokenHash, DateTime revokedAt, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            string? hash = startingTokenHash;
            while (hash is not null && visited.Add(hash))
            {
                // Read committed state, not the stale entity loaded before a competing rotation.
                // Lock before following the link so rotation cannot add an unseen descendant.
                var rows = await db.RefreshTokens.FromSqlInterpolated(
                    $"SELECT * FROM \"RefreshTokens\" WHERE \"UserId\" = {userId} AND \"TokenHash\" = {hash} FOR UPDATE")
                    .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
                var current = rows.SingleOrDefault();
                if (current is null)
                {
                    break;
                }

                await db.RefreshTokens.Where(token => token.Id == current.Id && token.UserId == userId && token.RevokedAt == null)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAt, revokedAt), cancellationToken).ConfigureAwait(false);
                hash = current.ReplacedByTokenHash;
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public Task<RefreshToken?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken) =>
        db.RefreshTokens.SingleOrDefaultAsync(token => token.Id == id && token.UserId == userId, cancellationToken);
}
