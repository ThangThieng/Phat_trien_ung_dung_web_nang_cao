using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

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

    public async Task<PagedResult<RecipeSearchResultDto>> SearchPublishedAsync(
        string tsQuery,
        int page,
        int pageSize,
        RecipeFilterSpec filter,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tsQuery);
        ArgumentNullException.ThrowIfNull(filter);

        // FR-SRCH-001 bước 4: SearchVector @@ to_tsquery('simple', :q) — tsQuery là THAM SỐ của câu lệnh, không ghép chuỗi SQL.
        var matches = ApplyFilter(Published(), filter)
            .Where(r => EF.Property<NpgsqlTsVector>(r, RecipeConfiguration.SearchVectorColumn)
                .Matches(EF.Functions.ToTsQuery(RecipeConfiguration.TextSearchConfig, tsQuery)));

        var total = await matches.CountAsync(cancellationToken).ConfigureAwait(false);

        // Bước 5: ORDER BY ts_rank DESC; tie-breaker ổn định để phân trang không lặp/bỏ sót bản ghi cùng điểm.
        var ranked = await matches
            .Select(r => new
            {
                r.Id,
                r.PublishedAt,
                Rank = EF.Property<NpgsqlTsVector>(r, RecipeConfiguration.SearchVectorColumn)
                    .Rank(EF.Functions.ToTsQuery(RecipeConfiguration.TextSearchConfig, tsQuery)),
            })
            .OrderByDescending(x => x.Rank)
            .ThenByDescending(x => x.PublishedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ids = ranked.Select(x => x.Id).ToList();
        var summaries = await ToSummaries(db.Recipes.AsNoTracking().Where(r => ids.Contains(r.Id)))
            .ToDictionaryAsync(s => s.Id, cancellationToken)
            .ConfigureAwait(false);

        var items = ranked
            .Where(x => summaries.ContainsKey(x.Id))
            .Select(x => RecipeSearchResultDto.From(summaries[x.Id], x.Rank))
            .ToList();

        return new PagedResult<RecipeSearchResultDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<RecipeSitemapEntryDto>> GetPublishedSitemapAsync(CancellationToken cancellationToken) =>
        await Published()
            .OrderBy(r => r.Slug)
            .Select(r => new RecipeSitemapEntryDto(r.Slug, r.UpdatedAt ?? r.PublishedAt ?? r.CreatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<PagedResult<RecipeSummaryDto>> GetByAuthorPagedAsync(
        string authorId,
        RecipeStatus? status,
        RecipeListCriteria criteria,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);
        ArgumentNullException.ThrowIfNull(criteria);

        // FR-RCP-011 bước 6: lọc theo tác giả (mọi trạng thái), thêm status nếu có.
        var query = db.Recipes.AsNoTracking().Where(r => r.AuthorId == authorId);
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return PageAsync(query, criteria, cancellationToken);
    }

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
