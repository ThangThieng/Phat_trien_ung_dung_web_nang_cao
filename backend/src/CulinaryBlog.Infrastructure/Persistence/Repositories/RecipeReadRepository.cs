using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Read-side cho Recipe: AsNoTracking + projection (list), split query (detail) – tránh N+1 (NFR-PERF-004).
/// Mọi phương thức <c>Published…</c> cố định <c>Status == Published</c> ngay trong repository và không nhận danh tính người gọi
/// (MT-34, retrofit D-4/D-5); Global Query Filter đã loại <c>IsDeleted</c>.
/// </summary>
public sealed class RecipeReadRepository(CulinaryBlogDbContext db) : IRecipeReadRepository
{
    public Task<PagedResult<RecipeSummaryDto>> GetPublishedPagedAsync(RecipeListCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        return PageAsync(Published(), criteria, cancellationToken);
    }

    public Task<RecipeDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken) =>
        DetailAsync(DetailQuery().Where(r => r.Slug == slug && r.Status == RecipeStatus.Published), cancellationToken);

    public Task<RecipeDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken) =>
        DetailAsync(DetailQuery().Where(r => r.Id == id), cancellationToken);

    private static IQueryable<Recipe> ApplyFilter(IQueryable<Recipe> query, RecipeFilterSpec filter)
    {
        // FR-SRCH-002: các tiêu chí kết hợp bằng AND; tiêu chí để trống thì bỏ qua.
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == filter.CategoryId.Value);
        }

        if (filter.Difficulty.HasValue)
        {
            query = query.Where(r => r.Difficulty == filter.Difficulty.Value);
        }

        if (filter.MaxCookTime.HasValue)
        {
            query = query.Where(r => r.CookTimeMinutes <= filter.MaxCookTime.Value);
        }

        if (filter.MaxPrepTime.HasValue)
        {
            query = query.Where(r => r.PrepTimeMinutes <= filter.MaxPrepTime.Value);
        }

        if (filter.MinServings.HasValue)
        {
            query = query.Where(r => r.Servings >= filter.MinServings.Value);
        }

        return query;
    }

    private static IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, RecipeSort sort)
    {
        // Biểu thức cột đến từ whitelist SortMapper (tầng Application) — không có đường nào ghép tên cột từ chuỗi người dùng.
        var ordered = sort.Descending ? query.OrderByDescending(sort.KeySelector) : query.OrderBy(sort.KeySelector);

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

    /// <summary>Tập công thức công khai: chỉ Published (MT-34).</summary>
    private IQueryable<Recipe> Published() =>
        db.Recipes.AsNoTracking().Where(r => r.Status == RecipeStatus.Published);

    private async Task<PagedResult<RecipeSummaryDto>> PageAsync(
        IQueryable<Recipe> query,
        RecipeListCriteria criteria,
        CancellationToken cancellationToken)
    {
        query = ApplyFilter(query, criteria.Filter);

        // COUNT trước khi phân trang (FR-RCP-001 bước 7)
        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await ToSummaries(ApplySorting(query, criteria.Sort)
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<RecipeSummaryDto>(items, total, criteria.Page, criteria.PageSize);
    }

    /// <summary>Projection card danh sách (FR-RCP-001) — một round-trip, không N+1. Giữ nguyên thứ tự của <paramref name="query"/>.</summary>
    private IQueryable<RecipeSummaryDto> ToSummaries(IQueryable<Recipe> query) =>
        query.Join(db.Users, r => r.AuthorId, u => u.Id, (r, u) => new RecipeSummaryDto(
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
            r.CreatedAt));

    private IQueryable<Recipe> DetailQuery() =>
        db.Recipes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Category)
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images);

    private async Task<RecipeDetailDto?> DetailAsync(IQueryable<Recipe> query, CancellationToken cancellationToken)
    {
        var recipe = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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

        return ToDetail(recipe, author);
    }
}
