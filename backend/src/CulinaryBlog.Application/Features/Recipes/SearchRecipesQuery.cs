using System.Globalization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-SRCH-001 — tìm kiếm toàn văn tiếng Việt không dấu trên công thức Published, xếp theo <c>ts_rank</c> giảm dần, kèm
/// <c>relevanceScore</c>. Bộ lọc là hai tham số SRS §8.3 cho phép (<c>categoryId</c>, <c>difficulty</c>) của
/// <see cref="RecipeFilterSpec"/> dùng chung. Redis cache-aside <c>search:{queryHash}</c>, TTL 1 phút, KHÔNG invalidate chủ động
/// — TTL ngắn nên để hết hạn tự nhiên (NFR-PERF-003).
/// </summary>
public sealed record SearchRecipesQuery(
    string Q,
    int Page = 1,
    int PageSize = RecipeQueryRules.DefaultPageSize,
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null) : IRequest<PagedResult<RecipeSearchResultDto>>, ICacheable
{
    public const int MinTermLength = 2;

    public RecipeFilterSpec Filter => new(CategoryId, Difficulty);

    /// <summary>Băm theo biểu thức tsquery đã chuẩn hóa: "Phở bò", "pho  bo" và "PHO BO" dùng chung một khóa.</summary>
    public string CacheKey => RecipeCacheKeys.Search(string.Create(
        CultureInfo.InvariantCulture,
        $"q={SearchTermBuilder.Build(Q)}&page={Page}&pageSize={PageSize}&{Filter.ToCacheSegment()}"));

    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}

public sealed class SearchRecipesQueryValidator : AbstractValidator<SearchRecipesQuery>
{
    public SearchRecipesQueryValidator()
    {
        // A1: q rỗng hoặc < 2 ký tự (không tính khoảng trắng hai đầu) → 400 VALIDATION_ERROR.
        RuleFor(x => x.Q)
            .Must(q => q.Trim().Length >= SearchRecipesQuery.MinTermLength)
            .OverridePropertyName("q")
            .WithMessage("Từ khóa tìm kiếm (q) phải có ít nhất 2 ký tự.");
        RuleFor(x => x.Page).ValidPage();
        RuleFor(x => x.PageSize).ValidPageSize();
        RuleFor(x => x.Difficulty).ValidDifficulty();
    }
}

public sealed class SearchRecipesQueryHandler(IRecipeReadRepository recipes)
    : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSearchResultDto>>
{
    public async Task<PagedResult<RecipeSearchResultDto>> Handle(SearchRecipesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // A3: sau khi làm sạch không còn từ nào (ví dụ q = "!!") → 200 với items rỗng, không gửi tsquery rỗng xuống DB.
        var tsQuery = SearchTermBuilder.Build(request.Q);
        if (tsQuery.Length == 0)
        {
            return new PagedResult<RecipeSearchResultDto>([], 0, request.Page, request.PageSize);
        }

        return await recipes
            .SearchPublishedAsync(tsQuery, request.Page, request.PageSize, request.Filter, cancellationToken)
            .ConfigureAwait(false);
    }
}
