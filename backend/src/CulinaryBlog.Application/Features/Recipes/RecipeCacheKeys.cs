using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// Khóa cache Redis của module Recipe (SRS NFR-PERF-003) — một nguồn cho cả query (ICacheable) lẫn command (ICacheInvalidator).
/// Tiền tố (kết thúc bằng ":") được khai báo sẵn; xóa theo tiền tố bằng SCAN là việc của Buổi 7, hiện
/// <c>CacheInvalidationBehavior</c> xóa theo khóa chính xác — tới lúc đó khóa <c>recipes:list:*</c> hết hạn tự nhiên sau 2 phút.
/// </summary>
public static class RecipeCacheKeys
{
    public const string ListPrefix = "recipes:list:";

    public const string SearchPrefix = "search:";

    /// <summary>MT-48: danh sách slug phục vụ sitemap (TTL 1 giờ).</summary>
    public const string Sitemap = "recipes:sitemap";

    public static string Detail(string slug) => $"recipe:{slug}";

    /// <summary>FR-RCP-001 — <c>recipes:list:{queryHash}</c>, TTL 2 phút (NFR-PERF-003).</summary>
    public static string List(string normalizedQuery) => ListPrefix + QueryHash.Compute(normalizedQuery);

    /// <summary>FR-SRCH-001 — <c>search:{queryHash}</c>, TTL 1 phút, không invalidate chủ động (NFR-PERF-003).</summary>
    public static string Search(string normalizedQuery) => SearchPrefix + QueryHash.Compute(normalizedQuery);

    /// <summary>Mọi khóa công khai bị ảnh hưởng khi NỘI DUNG của một công thức đổi (bước, nguyên liệu, ảnh, thông tin cơ bản).</summary>
    public static IReadOnlyCollection<string> ForContentChange(string slug) => [Detail(slug), ListPrefix, SearchPrefix];

    /// <summary>
    /// Mọi khóa công khai bị ảnh hưởng khi công thức vào/ra tập Published hoặc bị xóa (vòng đời — FR-RCP-005/006/007):
    /// danh sách, tìm kiếm, sitemap, chi tiết danh mục và <c>recipeCount</c> của danh sách danh mục.
    /// </summary>
    public static IReadOnlyCollection<string> ForVisibilityChange(string slug) =>
        [Detail(slug), ListPrefix, SearchPrefix, Sitemap, CategoryCacheKeys.DetailPrefix, CategoryCacheKeys.All];
}

/// <summary>
/// Gốc của command ghi một công thức ĐÃ CÓ: khóa cache cần xóa phụ thuộc slug — thứ chỉ biết sau khi handler nạp công thức.
/// <c>CacheInvalidationBehavior</c> đọc <see cref="CacheKeysToInvalidate"/> SAU KHI handler chạy thành công, nên handler
/// ghi khóa vào đây bằng <see cref="InvalidateOnSuccess"/>; handler ném lỗi thì không khóa nào bị xóa.
/// </summary>
public abstract record RecipeWriteCommand : ICacheInvalidator
{
    private readonly List<string> _cacheKeys = [];

    public IReadOnlyCollection<string> CacheKeysToInvalidate => _cacheKeys.AsReadOnly();

    public void InvalidateOnSuccess(IEnumerable<string> keys) => _cacheKeys.AddRange(keys);
}
