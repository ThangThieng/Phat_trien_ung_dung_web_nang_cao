using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Common.Behaviors;

/// <summary>
/// Pipeline #5 (SRS §6.3): sau khi Command implement <see cref="ICacheInvalidator"/> thành công → xóa cache liên quan theo cột
/// "Invalidate khi" của bảng TTL chuẩn (NFR-PERF-003).
///
/// Buổi 8 — audit cache: khóa kết thúc bằng <c>":"</c> được xóa theo tiền tố (<see cref="CacheInvalidation"/>). Trước Buổi 8 mọi
/// khóa đều đi qua <c>RemoveAsync</c>, nên "xóa theo prefix <c>recipes:list:</c>" thực chất xóa một khóa không tồn tại — danh
/// sách công khai chỉ được làm mới khi hết TTL 2 phút.
/// </summary>
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
            await cache.InvalidateAsync(invalidator.CacheKeysToInvalidate, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
