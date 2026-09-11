using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(CulinaryBlogDbContext db) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken) =>
        await db.RefreshTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);
}
