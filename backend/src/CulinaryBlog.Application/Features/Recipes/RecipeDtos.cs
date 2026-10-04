using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record CategoryRefDto(Guid Id, string Name, string Slug);

public sealed record AuthorDto(string Id, string DisplayName, string? AvatarUrl);

/// <summary>Card hiển thị trong danh sách (FR-RCP-001, FR-CAT-002).</summary>
public sealed record RecipeSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    RecipeStatus Status,
    string? PrimaryImageUrl,
    CategoryRefDto Category,
    AuthorDto Author,
    DateTime? PublishedAt,
    DateTime CreatedAt)
{
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
}

/// <summary>
/// FR-SRCH-001 — một kết quả tìm kiếm: đúng các trường của <see cref="RecipeSummaryDto"/> cộng <c>relevanceScore</c>
/// (<c>ts_rank</c>). Khai báo phẳng (không lồng <c>RecipeSummaryDto</c>) để JSON cùng hình dạng với danh sách công khai, và để
/// Redis đọc lại được bằng constructor vị trí.
/// </summary>
public sealed record RecipeSearchResultDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    RecipeStatus Status,
    string? PrimaryImageUrl,
    CategoryRefDto Category,
    AuthorDto Author,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    double RelevanceScore)
{
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;

    public static RecipeSearchResultDto From(RecipeSummaryDto summary, double relevanceScore)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new RecipeSearchResultDto(
            summary.Id,
            summary.Title,
            summary.Slug,
            summary.Description,
            summary.PrepTimeMinutes,
            summary.CookTimeMinutes,
            summary.Servings,
            summary.Difficulty,
            summary.Status,
            summary.PrimaryImageUrl,
            summary.Category,
            summary.Author,
            summary.PublishedAt,
            summary.CreatedAt,
            relevanceScore);
    }
}

/// <summary>MT-48 / NFR-SEO-003 — một dòng của <c>GET /recipes/sitemap</c>: <c>{ slug, updatedAt }</c> (nguồn <c>lastmod</c>).</summary>
public sealed record RecipeSitemapEntryDto(string Slug, DateTime UpdatedAt);

public sealed record RecipeNutritionDto(decimal? Calories, decimal? Protein, decimal? Carbohydrates, decimal? Fat, decimal? Fiber, decimal? Sodium);

public sealed record RecipeStepDto(Guid Id, int StepNumber, string Title, string Description, int? TimerMinutes, string? ImageUrl);

public sealed record RecipeIngredientDto(Guid Id, string Name, decimal? Quantity, string? Unit, string? Notes, int OrderIndex);

public sealed record RecipeImageDto(Guid Id, string OriginalUrl, string? MediumUrl, string? ThumbnailUrl, string? AltText, bool IsPrimary, int OrderIndex);

/// <summary>FR-RCP-002 – toàn bộ nested data của một công thức.</summary>
public sealed record RecipeDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string? Instructions,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    RecipeStatus Status,
    CategoryRefDto Category,
    AuthorDto Author,
    RecipeNutritionDto? Nutrition,
    IReadOnlyList<RecipeStepDto> Steps,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    IReadOnlyList<RecipeImageDto> Images,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string RowVersion)
{
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
}
