using System.Text.Json;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CulinaryBlog.Infrastructure.Caching;

/// <summary>
/// ICacheService trên Redis 7 (IDistributedCache + JSON). NFR-REL-002: Redis lỗi → log warning và coi như cache-miss,
/// hệ thống tiếp tục đọc từ PostgreSQL (graceful degradation).
/// </summary>
public sealed partial class RedisCacheService(
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    IOptions<RedisCacheOptions> options,
    ILogger<RedisCacheService> logger) : ICacheService
{
    /// <summary>Số khóa mỗi vòng SCAN (gợi ý COUNT) — đủ lớn để ít vòng lặp, đủ nhỏ để mỗi lệnh không chặn Redis lâu.</summary>
    private const int ScanPageSize = 250;

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

    /// <summary>
    /// Buổi 8 — audit cache: tìm khóa bằng <c>SCAN … MATCH {instance}{prefix}*</c> (StackExchange.Redis tự dùng SCAN cho
    /// <see cref="IServer.KeysAsync(int, RedisValue, int, long, int, CommandFlags)"/> trên Redis ≥ 2.8) rồi <c>DEL</c> theo lô.
    /// Không dùng <c>KEYS</c>: lệnh O(N) chặn mọi client khác trong lúc chạy. Tên khóa thật trong Redis mang tiền tố instance
    /// <c>culinaryblog:</c> (<see cref="RedisCacheOptions.InstanceName"/>) mà <see cref="IDistributedCache"/> tự thêm, nên mẫu MATCH
    /// phải có nó. Ký tự đại diện của glob trong tiền tố được thoát để tiền tố luôn khớp đúng chữ.
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        var pattern = EscapeGlob((options.Value.InstanceName ?? string.Empty) + prefix) + "*";
        try
        {
            foreach (var endpoint in redis.GetEndPoints())
            {
                var server = redis.GetServer(endpoint);
                if (!server.IsConnected || server.IsReplica)
                {
                    continue;
                }

                var batch = new List<RedisKey>(ScanPageSize);
                await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: ScanPageSize).WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    batch.Add(key);
                    if (batch.Count == ScanPageSize)
                    {
                        await DeleteAsync(batch).ConfigureAwait(false);
                    }
                }

                await DeleteAsync(batch).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCacheFailure(logger, "REMOVE-PREFIX", prefix, ex);
        }
    }

    private async Task DeleteAsync(List<RedisKey> batch)
    {
        if (batch.Count == 0)
        {
            return;
        }

        // Một lệnh DEL cho cả lô thay vì một round-trip mỗi khóa; giá trị là JSON nhỏ nên DEL không chặn đáng kể.
        await redis.GetDatabase().KeyDeleteAsync([.. batch]).ConfigureAwait(false);
        batch.Clear();
    }

    private static string EscapeGlob(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("*", @"\*", StringComparison.Ordinal)
            .Replace("?", @"\?", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal)
            .Replace("]", @"\]", StringComparison.Ordinal);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis cache {Operation} failed for key {CacheKey} – falling back to database")]
    private static partial void LogCacheFailure(ILogger logger, string operation, string cacheKey, Exception exception);
}
