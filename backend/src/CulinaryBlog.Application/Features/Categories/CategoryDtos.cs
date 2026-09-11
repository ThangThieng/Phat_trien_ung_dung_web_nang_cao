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
    /// <summary>Key theo FR-CAT-001 bước 3. Command Create/Update/Delete (Buổi 2) sẽ invalidate key này.</summary>
    public const string All = "categories:all";
}
