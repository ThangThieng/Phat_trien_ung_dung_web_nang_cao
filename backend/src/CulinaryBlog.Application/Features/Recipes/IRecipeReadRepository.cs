using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>Trường được phép sắp xếp (whitelist – chống injection qua sort param).</summary>
public enum RecipeSortField
{
    CreatedAt,
    Title,
    CookTime,
    PublishedAt,
}

/// <summary>
/// Phạm vi nhìn thấy theo FR-RCP-001 bước 4: Guest → Published; Author → Published + recipe của chính mình; Admin → tất cả.
/// </summary>
public sealed record RecipeVisibility(string? ViewerId, bool IsAdmin)
{
    public static RecipeVisibility PublicOnly { get; } = new(null, false);
}

public sealed record RecipeListCriteria(
    int Page,
    int PageSize,
    RecipeVisibility Visibility,
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null,
    int? MaxCookTime = null,
    RecipeSortField SortBy = RecipeSortField.CreatedAt,
    bool Descending = true);

/// <summary>Read-side (CQRS Query) – implementation dùng EF Core projection, AsNoTracking, không N+1 (NFR-PERF-004).</summary>
public interface IRecipeReadRepository
{
    Task<PagedResult<RecipeSummaryDto>> GetPagedAsync(RecipeListCriteria criteria, CancellationToken cancellationToken);

    /// <summary>Trả về detail kèm AuthorId để handler kiểm tra quyền xem Draft/Archived.</summary>
    Task<(RecipeDetailDto Recipe, string AuthorId)?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}
