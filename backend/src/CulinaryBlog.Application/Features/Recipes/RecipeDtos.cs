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
    string Instructions,
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
