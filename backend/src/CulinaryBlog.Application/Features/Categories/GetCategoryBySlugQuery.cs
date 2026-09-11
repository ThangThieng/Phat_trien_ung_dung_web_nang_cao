using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>FR-CAT-002 – chi tiết danh mục + recipes phân trang (Guest: Published; Author: + Draft của mình).</summary>
public sealed record GetCategoryBySlugQuery(string Slug, int Page = 1, int PageSize = 12) : IRequest<CategoryDetailDto>;

public sealed class GetCategoryBySlugQueryValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("page phải >= 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("pageSize phải trong khoảng 1–50.");
    }
}

public sealed class GetCategoryBySlugQueryHandler(
    ICategoryReadRepository categories,
    IRecipeReadRepository recipes,
    ICurrentUser currentUser)
    : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto>
{
    public async Task<CategoryDetailDto> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var category = await categories.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(ErrorCodes.CategoryNotFound, $"Không tìm thấy danh mục '{request.Slug}'.");

        var criteria = new RecipeListCriteria(
            request.Page,
            request.PageSize,
            new RecipeVisibility(currentUser.UserId, currentUser.IsAdmin),
            CategoryId: category.Id);

        var page = await recipes.GetPagedAsync(criteria, cancellationToken).ConfigureAwait(false);
        return new CategoryDetailDto(category, page);
    }
}
