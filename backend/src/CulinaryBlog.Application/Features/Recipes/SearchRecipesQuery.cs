using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>FR-SRCH-001: chỉ tìm công thức Published, cache theo truy vấn trong một phút.</summary>
public sealed record SearchRecipesQuery(string Q, int Page = 1, int PageSize = 12, Guid? CategoryId = null, RecipeDifficulty? Difficulty = null)
    : IRequest<PagedResult<RecipeSearchResultDto>>, ICacheable
{
    public string CacheKey => RecipeCacheKeys.SearchPrefix + string.Join(":", SearchTermBuilder.Build(Q), Page, PageSize, CategoryId, Difficulty);

    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}

public sealed class SearchRecipesQueryValidator : AbstractValidator<SearchRecipesQuery>
{
    public SearchRecipesQueryValidator()
    {
        RuleFor(x => x.Q).Must(value => value is not null && value.Trim().Length >= 2).OverridePropertyName("q");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Difficulty).IsInEnum().When(x => x.Difficulty.HasValue);
    }
}

public sealed class SearchRecipesQueryHandler(IRecipeReadRepository recipes)
    : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSearchResultDto>>
{
    public Task<PagedResult<RecipeSearchResultDto>> Handle(SearchRecipesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var tsQuery = SearchTermBuilder.Build(request.Q);
        return tsQuery.Length == 0
            ? Task.FromResult(new PagedResult<RecipeSearchResultDto>([], 0, request.Page, request.PageSize))
            : recipes.SearchPublishedAsync(tsQuery, request.Page, request.PageSize, request.CategoryId, request.Difficulty, cancellationToken);
    }
}
