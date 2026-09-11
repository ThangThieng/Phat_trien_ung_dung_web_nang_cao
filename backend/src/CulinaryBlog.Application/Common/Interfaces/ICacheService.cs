namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Distributed cache (Redis). Khi Redis lỗi, implementation phải trả về cache-miss thay vì ném exception (NFR-REL-002).</summary>
public interface ICacheService
{
    Task<CacheEntry<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public readonly record struct CacheEntry<T>(bool Found, T? Value);
