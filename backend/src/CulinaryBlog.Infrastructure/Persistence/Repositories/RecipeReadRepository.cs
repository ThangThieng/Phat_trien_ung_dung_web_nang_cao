using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>Read-side cho Recipe: AsNoTracking + projection (list), split query (detail) – tránh N+1 (NFR-PERF-004).</summary>
public sealed class RecipeReadRepository(CulinaryBlogDbContext db) : IRecipeReadRepository
{
    public async Task<PagedResult<RecipeSummaryDto>> GetPagedAsync(RecipeListCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = ApplyVisibility(db.Recipes.AsNoTracking(), criteria.Visibility);

        if (criteria.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == criteria.CategoryId.Value);
        }

        if (criteria.Difficulty.HasValue)
        {
            query = query.Where(r => r.Difficulty == criteria.Difficulty.Value);
        }

        if (criteria.MaxCookTime.HasValue)
        {
            query = query.Where(r => r.CookTimeMinutes <= criteria.MaxCookTime.Value);
        }

        // Bước 6: COUNT trước khi phân trang
        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await ApplySorting(query, criteria.SortBy, criteria.Descending)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Join(db.Users, r => r.AuthorId, u => u.Id, (r, u) => new RecipeSummaryDto(
                r.Id,
                r.Title,
                r.Slug,
                r.Description,
                r.PrepTimeMinutes,
                r.CookTimeMinutes,
                r.Servings,
                r.Difficulty,
                r.Status,
                r.Images.Where(i => i.IsPrimary).Select(i => i.ThumbnailUrl ?? i.OriginalUrl).FirstOrDefault(),
                new CategoryRefDto(r.CategoryId, r.Category!.Name, r.Category.Slug),
                new AuthorDto(u.Id, u.DisplayName, u.AvatarUrl),
                r.PublishedAt,
                r.CreatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<RecipeSummaryDto>(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task<(RecipeDetailDto Recipe, string AuthorId)?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Category)
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Slug == slug, cancellationToken)
            .ConfigureAwait(false);

        if (recipe is null)
        {
            return null;
        }

        var author = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == recipe.AuthorId)
            .Select(u => new AuthorDto(u.Id, u.DisplayName, u.AvatarUrl))
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        return (ToDetail(recipe, author), recipe.AuthorId);
    }

    /// <summary>FR-RCP-001 bước 4: Guest → Published; Author → Published OR của mình; Admin → tất cả.</summary>
    private static IQueryable<Recipe> ApplyVisibility(IQueryable<Recipe> query, RecipeVisibility visibility)
    {
        if (visibility.IsAdmin)
        {
            return query;
        }

        return visibility.ViewerId is null
            ? query.Where(r => r.Status == RecipeStatus.Published)
            : query.Where(r => r.Status == RecipeStatus.Published || r.AuthorId == visibility.ViewerId);
    }

    private static IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, RecipeSortField sortBy, bool descending)
    {
        var ordered = (sortBy, descending) switch
        {
            (RecipeSortField.Title, false) => query.OrderBy(r => r.Title),
            (RecipeSortField.Title, true) => query.OrderByDescending(r => r.Title),
            (RecipeSortField.CookTime, false) => query.OrderBy(r => r.CookTimeMinutes),
            (RecipeSortField.CookTime, true) => query.OrderByDescending(r => r.CookTimeMinutes),
            (RecipeSortField.PublishedAt, false) => query.OrderBy(r => r.PublishedAt),
            (RecipeSortField.PublishedAt, true) => query.OrderByDescending(r => r.PublishedAt),
            (_, false) => query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt),
        };

        // Tie-breaker ổn định để phân trang không lặp/bỏ sót
        return ordered.ThenBy(r => r.Id);
    }

    private static RecipeDetailDto ToDetail(Recipe r, AuthorDto author)
    {
        var nutrition = r.Nutrition is null
            ? null
            : new RecipeNutritionDto(r.Nutrition.Calories, r.Nutrition.Protein, r.Nutrition.Carbohydrates, r.Nutrition.Fat, r.Nutrition.Fiber, r.Nutrition.Sodium);
        var images = r.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.OrderIndex)
            .Select(i => new RecipeImageDto(i.Id, i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl, i.AltText, i.IsPrimary, i.OrderIndex))
            .ToList();

        return new RecipeDetailDto(
            r.Id,
            r.Title,
            r.Slug,
            r.Description,
            r.Instructions,
            r.PrepTimeMinutes,
            r.CookTimeMinutes,
            r.Servings,
            r.Difficulty,
            r.Status,
            new CategoryRefDto(r.CategoryId, r.Category?.Name ?? string.Empty, r.Category?.Slug ?? string.Empty),
            author,
            nutrition,
            [.. r.Steps.OrderBy(s => s.StepNumber).Select(s => new RecipeStepDto(s.Id, s.StepNumber, s.Title, s.Description, s.TimerMinutes, s.ImageUrl))],
            [.. r.Ingredients.OrderBy(i => i.OrderIndex).Select(i => new RecipeIngredientDto(i.Id, i.Name, i.Quantity, i.Unit, i.Notes, i.OrderIndex))],
            images,
            r.PublishedAt,
            r.CreatedAt,
            r.UpdatedAt,
            Convert.ToBase64String(r.RowVersion));
    }
}
