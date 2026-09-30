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

    public Task<RefreshToken?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken) =>
        db.RefreshTokens.SingleOrDefaultAsync(token => token.Id == id && token.UserId == userId, cancellationToken);
}
