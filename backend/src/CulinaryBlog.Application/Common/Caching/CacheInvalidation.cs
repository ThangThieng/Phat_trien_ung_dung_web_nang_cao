using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Application.Common.Caching;

/// <summary>
/// Quy ước khóa cache dùng chung giữa <see cref="ICacheInvalidator"/>, <c>CacheInvalidationBehavior</c> và job ghi DB ngoài pipeline
/// MediatR (Buổi 8 — audit cache): khóa kết thúc bằng <c>":"</c> là <b>tiền tố</b> của cả một họ khóa <c>{prefix}{queryHash}</c>
/// (<c>recipes:list:</c>, <c>categories:detail:</c>) → xóa bằng SCAN; còn lại là khóa chính xác.
/// </summary>
public static class CacheInvalidation
{
    public static bool IsPrefix(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key.EndsWith(':');
    }

    /// <summary>Xóa từng khóa/tiền tố một lần (bỏ trùng). Redis lỗi thì <see cref="ICacheService"/> nuốt lỗi — khóa sót hết hạn theo TTL.</summary>
    public static async Task InvalidateAsync(this ICacheService cache, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys.Distinct(StringComparer.Ordinal))
        {
            if (IsPrefix(key))
            {
                await cache.RemoveByPrefixAsync(key, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
