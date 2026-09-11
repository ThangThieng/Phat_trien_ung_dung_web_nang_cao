namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Query implement interface này sẽ được CachingBehavior cache vào Redis (SRS §6.3).</summary>
public interface ICacheable
{
    string CacheKey { get; }

    TimeSpan Expiration { get; }
}

/// <summary>Command implement interface này sẽ xóa các cache key liên quan sau khi thành công (SRS §6.3).</summary>
public interface ICacheInvalidator
{
    IReadOnlyCollection<string> CacheKeysToInvalidate { get; }
}
