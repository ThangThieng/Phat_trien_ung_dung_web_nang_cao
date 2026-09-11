using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Common.Behaviors;

/// <summary>Pipeline #3 (SRS §6.3): Query implement <see cref="ICacheable"/> được đọc/ghi Redis theo cache-aside.</summary>
public sealed class CachingBehavior<TRequest, TResponse>(ICacheService cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ICacheable cacheable)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var cached = await cache.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken).ConfigureAwait(false);
        if (cached.Found && cached.Value is not null)
        {
            return cached.Value;
        }

        var response = await next(cancellationToken).ConfigureAwait(false);
        await cache.SetAsync(cacheable.CacheKey, response, cacheable.Expiration, cancellationToken).ConfigureAwait(false);
        return response;
    }
}
