using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>FR-RCP-011 – danh sách riêng tư của Author, gồm mọi trạng thái và tuyệt đối không dùng cache chung.</summary>
public sealed record GetMyRecipesQuery(
    RecipeStatus? Status = null,
    int Page = 1,
    int PageSize = 12,
    string? Sort = null) : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetMyRecipesQueryValidator : AbstractValidator<GetMyRecipesQuery>
{
    public GetMyRecipesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("page phải >= 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("pageSize phải trong khoảng 1–50.");
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Sort)
            .Must(s => RecipeSortParser.TryParse(s, out _, out _))
            .WithMessage("sort chỉ chấp nhận: createdAt, title, cookTime, publishedAt (tiền tố '-' = giảm dần).");
    }
}

public sealed class GetMyRecipesQueryHandler(IRecipeReadRepository recipes, ICurrentUser currentUser)
    : IRequestHandler<GetMyRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(GetMyRecipesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var authorId = currentUser.UserId ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập.");
        _ = RecipeSortParser.TryParse(request.Sort, out var sortBy, out var descending);
        return recipes.GetMinePagedAsync(authorId, request.Status, request.Page, request.PageSize, sortBy, descending, cancellationToken);
    }
}
