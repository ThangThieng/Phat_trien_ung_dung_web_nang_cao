using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-RCP-001 – danh sách công thức phân trang + lọc + sắp xếp.
/// Sort theo cú pháp FR-SRCH-003: "-createdAt" (giảm dần) | "title" | "-cookTime"...
/// </summary>
public sealed record GetRecipesQuery(
    int Page = 1,
    int PageSize = 12,
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null,
    int? MaxCookTime = null,
    string? Sort = null) : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetRecipesQueryValidator : AbstractValidator<GetRecipesQuery>
{
    public GetRecipesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("page phải >= 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("pageSize phải trong khoảng 1–50.");
        RuleFor(x => x.MaxCookTime).GreaterThanOrEqualTo(0).When(x => x.MaxCookTime.HasValue);
        RuleFor(x => x.Difficulty).IsInEnum().When(x => x.Difficulty.HasValue);
        RuleFor(x => x.Sort)
            .Must(s => RecipeSortParser.TryParse(s, out _, out _))
            .WithMessage("sort chỉ chấp nhận: createdAt, title, cookTime, publishedAt (tiền tố '-' = giảm dần).");
    }
}

public sealed class GetRecipesQueryHandler(IRecipeReadRepository recipes, ICurrentUser currentUser)
    : IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        _ = RecipeSortParser.TryParse(request.Sort, out var sortBy, out var descending);

        var criteria = new RecipeListCriteria(
            request.Page,
            request.PageSize,
            new RecipeVisibility(currentUser.UserId, currentUser.IsAdmin),
            request.CategoryId,
            request.Difficulty,
            request.MaxCookTime,
            sortBy,
            descending);

        return recipes.GetPagedAsync(criteria, cancellationToken);
    }
}
