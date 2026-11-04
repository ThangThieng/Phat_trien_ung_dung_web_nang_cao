using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// Khóa cache Redis của module Recipe (SRS NFR-PERF-003) — một nguồn cho cả query (ICacheable) lẫn command (ICacheInvalidator).
/// Khóa kết thúc bằng ":" là tiền tố: <c>CacheInvalidationBehavior</c> xóa cả họ khóa bằng SCAN (Buổi 8 — audit cache,
/// <c>docs/cache-report.md</c>).
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

    /// <summary>
    /// Khóa bị ảnh hưởng khi NỘI DUNG một công thức đổi (bước, nguyên liệu, ảnh, thông tin cơ bản) — cột "Invalidate khi" của
    /// NFR-PERF-003 sau CR-2026-06 (SRS v1.2.4, MT-69): họ khóa công khai <c>recipes:list:</c> và <c>categories:detail:</c> chỉ bị xóa
    /// khi thay đổi CHẠM TỚI TẬP PUBLISHED. Bản nháp/lưu trữ không bao giờ xuất hiện ở endpoint công khai (MT-34), nên sửa chúng mà xóa
    /// danh sách công khai chỉ làm mất cache vô ích — đo Buổi 8: 10 % lượt ghi lên bản nháp kéo hit rate từ 97,9 % xuống 75,4 %.
    /// <c>recipe:{slug}</c> vẫn luôn bị xóa (rẻ, và an toàn khi trạng thái vừa đổi trong cùng phiên làm việc).
    /// <c>search:</c> CỐ Ý không có: SRS ghi "Không invalidate — để hết hạn tự nhiên" (TTL 1 phút).
    /// </summary>
    public static IReadOnlyCollection<string> ForContentChange(string slug, RecipeStatus status) =>
        status == RecipeStatus.Published
            ? [Detail(slug), ListPrefix, CategoryCacheKeys.DetailPrefix]
            : [Detail(slug)];

    /// <summary>
    /// Khóa bị ảnh hưởng bởi một thao tác vòng đời (FR-RCP-005/006/007): công thức VÀO hoặc RA tập Published (trạng thái trước hoặc sau
    /// là Published; xóa mềm một công thức Published cũng là "ra") → chi tiết, danh sách, sitemap, chi tiết danh mục và
    /// <c>recipeCount</c> của <c>categories:all</c> (chỉ đếm Published). Thao tác không chạm tập Published (lưu trữ/xóa một bản nháp,
    /// khôi phục bản lưu trữ về nháp) chỉ xóa <c>recipe:{slug}</c> — CR-2026-06.
    /// </summary>
    public static IReadOnlyCollection<string> ForVisibilityChange(string slug, RecipeStatus before, RecipeStatus after) =>
        before == RecipeStatus.Published || after == RecipeStatus.Published
            ? [Detail(slug), ListPrefix, Sitemap, CategoryCacheKeys.DetailPrefix, CategoryCacheKeys.All]
            : [Detail(slug)];

    /// <summary>
    /// Công thức chuyển sang danh mục khác (FR-RCP-004): <c>recipeCount</c> của cả hai danh mục trong <c>categories:all</c> chỉ đổi khi
    /// công thức đang Published.
    /// </summary>
    public static IReadOnlyCollection<string> ForCategoryChange(RecipeStatus status) =>
        status == RecipeStatus.Published ? [CategoryCacheKeys.All] : [];
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
