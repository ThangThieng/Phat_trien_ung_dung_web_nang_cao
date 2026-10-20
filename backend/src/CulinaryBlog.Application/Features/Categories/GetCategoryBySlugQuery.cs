using System.Globalization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Exceptions.Categories;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>
/// FR-CAT-002 — chi tiết danh mục + công thức phân trang. Endpoint hoàn toàn CÔNG KHAI: chỉ công thức <c>Published</c> cho mọi
/// người gọi, không đọc danh tính (MT-34, retrofit D-5). Redis cache-aside <c>categories:detail:{slug}:{queryHash}</c>,
/// TTL 2 phút (NFR-PERF-003, retrofit D-6).
/// </summary>
public sealed record GetCategoryBySlugQuery(
    string Slug,
    int Page = 1,
    int PageSize = RecipeQueryRules.DefaultPageSize,
    string? SortBy = null,
    string? SortOrder = null) : IRequest<CategoryDetailDto>, ICacheable
{
    public string CacheKey => CategoryCacheKeys.Detail(Slug, NormalizedQuery);

    public TimeSpan Expiration => TimeSpan.FromMinutes(2);

    /// <summary>Tham số phân trang/sắp xếp chuẩn hóa không phân biệt hoa/thường — đầu vào của <c>{queryHash}</c>.</summary>
    private string NormalizedQuery => string.Create(
        CultureInfo.InvariantCulture,
        $"page={Page}&pageSize={PageSize}&sortBy={SortBy ?? SortMapper.DefaultSortBy}&sortOrder={SortOrder ?? SortMapper.DefaultSortOrder}")
        .ToUpperInvariant();
}

public sealed class GetCategoryBySlugQueryValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Page).ValidPage();
        RuleFor(x => x.PageSize).ValidPageSize();
        RuleFor(x => x.SortBy).ValidSortBy();
        RuleFor(x => x.SortOrder).ValidSortOrder();
    }
}

public sealed class GetCategoryBySlugQueryHandler(ICategoryReadRepository categories, IRecipeReadRepository recipes)
    : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto>
{
    public async Task<CategoryDetailDto> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categories.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false)
            ?? throw new CategoryNotFoundException(request.Slug);

        var criteria = new RecipeListCriteria(
            request.Page,
            request.PageSize,
            new RecipeFilterSpec(CategoryId: category.Id),
            SortMapper.Map(request.SortBy, request.SortOrder));

        var page = await recipes.GetPublishedPagedAsync(criteria, cancellationToken).ConfigureAwait(false);
        return new CategoryDetailDto(category, page);
    }
}
