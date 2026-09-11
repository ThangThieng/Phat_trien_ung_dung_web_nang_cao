using System.Text.Json;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Caching;

/// <summary>
/// ICacheService trên Redis 7 (IDistributedCache + JSON). NFR-REL-002: Redis lỗi → log warning và coi như cache-miss,
/// hệ thống tiếp tục đọc từ PostgreSQL (graceful degradation).
/// </summary>
public sealed partial class RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CacheEntry<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            return bytes is null
                ? new CacheEntry<T>(false, default)
                : new CacheEntry<T>(true, JsonSerializer.Deserialize<T>(bytes, JsonOptions));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCacheFailure(logger, "GET", key, ex);
            return new CacheEntry<T>(false, default);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCacheFailure(logger, "SET", key, ex);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCacheFailure(logger, "REMOVE", key, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis cache {Operation} failed for key {CacheKey} – falling back to database")]
    private static partial void LogCacheFailure(ILogger logger, string operation, string cacheKey, Exception exception);
}
