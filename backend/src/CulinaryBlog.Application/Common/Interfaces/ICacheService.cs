namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Distributed cache (Redis). Khi Redis lỗi, implementation phải trả về cache-miss thay vì ném exception (NFR-REL-002).</summary>
public interface ICacheService
{
    Task<CacheEntry<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa mọi khóa bắt đầu bằng <paramref name="prefix"/> (ví dụ <c>recipes:list:</c> — bảng TTL chuẩn NFR-PERF-003, "xóa theo
    /// prefix"). Implementation dùng <c>SCAN</c>, không dùng <c>KEYS</c> (lệnh O(N) chặn Redis trong lúc chạy). Redis lỗi thì bỏ qua như
    /// <see cref="RemoveAsync"/> — khóa còn sót hết hạn theo TTL.
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}

public readonly record struct CacheEntry<T>(bool Found, T? Value);
