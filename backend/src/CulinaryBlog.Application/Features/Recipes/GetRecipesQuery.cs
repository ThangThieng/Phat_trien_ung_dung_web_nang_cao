using System.Globalization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-RCP-001 (+ FR-SRCH-002/003/004) — danh sách công thức CÔNG KHAI: chỉ <c>Status == Published</c> cho mọi người gọi, kể
/// cả Admin, và không đọc danh tính (MT-34, retrofit D-4). Nhờ vậy response thuần công khai và được cache dùng chung an toàn
/// qua Redis cache-aside <c>recipes:list:{queryHash}</c>, TTL 2 phút (retrofit D-6 — thay Output Cache).
/// <c>LegacySort</c> mang giá trị tham số <c>sort</c> cũ nếu client còn gửi (quy ước <c>sort=-field</c> đã bị loại bỏ — MT-01,
/// retrofit D-10): có giá trị là 400, vì FR-SRCH-003 cấm im lặng bỏ qua tham số sắp xếp sai — client cũ sẽ tưởng đang sắp xếp đúng.
/// </summary>
public sealed record GetRecipesQuery(
    int Page = 1,
    int PageSize = RecipeQueryRules.DefaultPageSize,
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null,
    int? MaxCookTime = null,
    int? MaxPrepTime = null,
    int? MinServings = null,
    string? SortBy = null,
    string? SortOrder = null,
    string? LegacySort = null) : IRequest<PagedResult<RecipeSummaryDto>>, ICacheable
{
    public RecipeFilterSpec Filter => new(CategoryId, Difficulty, MaxCookTime, MaxPrepTime, MinServings);

    public string CacheKey => RecipeCacheKeys.List(NormalizedQuery);

    public TimeSpan Expiration => TimeSpan.FromMinutes(2);

    /// <summary>Chuẩn hóa không phân biệt hoa/thường (whitelist sắp xếp so khớp OrdinalIgnoreCase) — đầu vào của <c>{queryHash}</c>.</summary>
    private string NormalizedQuery => string.Create(
        CultureInfo.InvariantCulture,
        $"page={Page}&pageSize={PageSize}&{Filter.ToCacheSegment()}&sortBy={SortBy ?? SortMapper.DefaultSortBy}&sortOrder={SortOrder ?? SortMapper.DefaultSortOrder}")
        .ToUpperInvariant();
}

public sealed class GetRecipesQueryValidator : AbstractValidator<GetRecipesQuery>
{
    public GetRecipesQueryValidator()
    {
        RuleFor(x => x.Page).ValidPage();
        RuleFor(x => x.PageSize).ValidPageSize();
        RuleFor(x => x.Difficulty).ValidDifficulty();
        RuleFor(x => x.MaxCookTime).NonNegativeMinutes();
        RuleFor(x => x.MaxPrepTime).NonNegativeMinutes();
        RuleFor(x => x.MinServings).PositiveServings();
        RuleFor(x => x.SortBy).ValidSortBy();
        RuleFor(x => x.SortOrder).ValidSortOrder();
        RuleFor(x => x.LegacySort)
            .Null()
            .OverridePropertyName("sort")
            .WithMessage("Tham số 'sort' đã bị loại bỏ — dùng sortBy (createdAt, publishedAt, title, cookTime, prepTime) và sortOrder (asc, desc).");
    }
}

public sealed class GetRecipesQueryHandler(IRecipeReadRepository recipes)
    : IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var criteria = new RecipeListCriteria(
            request.Page,
            request.PageSize,
            request.Filter,
            SortMapper.Map(request.SortBy, request.SortOrder));

        return recipes.GetPublishedPagedAsync(criteria, cancellationToken);
    }
}
