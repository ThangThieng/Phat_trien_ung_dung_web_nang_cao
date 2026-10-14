using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>FR-RCP-011: dữ liệu riêng tư, không dùng cache dùng chung.</summary>
public sealed record GetMyRecipesQuery(
    RecipeStatus? Status = null,
    int Page = 1,
    int PageSize = 12,
    string? Sort = null,
    string? SortBy = null,
    string? SortOrder = null,
    Guid? AuthorId = null)
    : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetMyRecipesQueryValidator : AbstractValidator<GetMyRecipesQuery>
{
    public GetMyRecipesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Sort).Must(value => RecipeSortParser.TryParse(value, out _, out _));
        RuleFor(x => x.SortBy).Must(value => RecipeSortParser.TryParse(value, out _, out _));
        RuleFor(x => x.SortOrder).Must(value => value is null or "asc" or "desc");
    }
}

public sealed class GetMyRecipesQueryHandler(IRecipeReadRepository recipes, ICurrentUser currentUser)
    : IRequestHandler<GetMyRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(GetMyRecipesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var authorId = currentUser.UserId ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập.");
        if (request.AuthorId.HasValue)
        {
            if (!currentUser.IsAdmin)
            {
                throw new ForbiddenException(ErrorCodes.RecipeForbidden, "Chỉ Admin được lọc theo authorId.");
            }

            authorId = request.AuthorId.Value.ToString();
        }

        var sort = request.Sort;
        if (request.SortBy is not null)
        {
            var prefix = request.SortOrder == "desc" ? "-" : string.Empty;
            sort = prefix + request.SortBy;
        }

        _ = RecipeSortParser.TryParse(sort, out var sortBy, out var descending);
        return recipes.GetMinePagedAsync(authorId, request.Status, request.Page, request.PageSize, sortBy, descending, cancellationToken);
    }
}
