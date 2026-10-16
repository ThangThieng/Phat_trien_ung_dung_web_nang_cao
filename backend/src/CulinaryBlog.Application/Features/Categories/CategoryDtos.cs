using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>FR-CAT-001 – { id, name, slug, description, imageUrl, recipeCount } (recipeCount chỉ đếm Published).</summary>
public sealed record CategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl, int OrderIndex, int RecipeCount);

/// <summary>FR-CAT-002 – { category, recipes: PagedResult }.</summary>
public sealed record CategoryDetailDto(CategoryDto Category, PagedResult<RecipeSummaryDto> Recipes);

public interface ICategoryReadRepository
{
    Task<IReadOnlyList<CategoryDto>> GetAllWithRecipeCountAsync(CancellationToken cancellationToken);

    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}

public static class CategoryCacheKeys
{
    /// <summary>Key theo FR-CAT-001 bước 3. Command Create/Update/Delete (Buổi 3) invalidate key này.</summary>
    public const string All = "categories:all";

    /// <summary>
    /// Tiền tố khóa cache chi tiết danh mục <c>categories:detail:{slug}:{queryHash}</c> (SRS NFR-PERF-003, TTL 2 phút).
    /// <c>GetCategoryBySlugQuery</c> cache dưới tiền tố này từ Buổi 4 (retrofit D-5/D-6). Ba command Create/Update/Delete khai báo
    /// sẵn tiền tố; <c>CacheInvalidationBehavior</c> hiện xóa theo khóa chính xác (thao tác chưa có tác dụng) nên khóa chi tiết
    /// hết hạn tự nhiên sau 2 phút, cho tới khi có <c>RemoveByPrefixAsync</c> (SCAN) ở Buổi 7.
    /// </summary>
    public const string DetailPrefix = "categories:detail:";

    /// <summary>FR-CAT-002 — <c>categories:detail:{slug}:{queryHash}</c>.</summary>
    public static string Detail(string slug, string normalizedQuery) => $"{DetailPrefix}{slug}:{QueryHash.Compute(normalizedQuery)}";
}

/// <summary>
/// SRS §7.9 Bảng Giới hạn Dữ liệu Chuẩn (MT-37) – nguồn sự thật duy nhất cho độ dài field Category.
/// Validator và định nghĩa cột DB PHẢI bằng nhau: Name varchar(100), Slug varchar(120), ImageUrl varchar(500).
/// </summary>
public static class CategoryLimits
{
    public const int NameMin = 2;
    public const int NameMax = 100;
    public const int DescriptionMax = 2000;
    public const int ImageUrlMax = 500;
}
