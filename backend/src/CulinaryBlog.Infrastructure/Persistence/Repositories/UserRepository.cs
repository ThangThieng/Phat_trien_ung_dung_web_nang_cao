using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CulinaryBlogDbContext db) : IUserRepository
{
    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken) =>
        await db.RefreshTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);

    public Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public Task RevokeRefreshTokenAsync(RefreshToken token, DateTime revokedAt, CancellationToken cancellationToken)
    {
        token.Revoke(revokedAt);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<string>> GetNormalizedUserNamesByPrefixAsync(string normalizedPrefix, CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking()
            .Where(user => user.NormalizedUserName != null && user.NormalizedUserName.StartsWith(normalizedPrefix))
            .Select(user => user.NormalizedUserName!)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
}
