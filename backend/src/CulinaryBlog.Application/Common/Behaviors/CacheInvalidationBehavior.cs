using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Common.Behaviors;

/// <summary>Pipeline #5 (SRS §6.3): sau khi Command implement <see cref="ICacheInvalidator"/> thành công → xóa cache liên quan.</summary>
public sealed class CacheInvalidationBehavior<TRequest, TResponse>(ICacheService cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var response = await next(cancellationToken).ConfigureAwait(false);

        if (request is ICacheInvalidator invalidator)
        {
            foreach (var key in invalidator.CacheKeysToInvalidate)
            {
                await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            }
        }

        return response;
    }
}
