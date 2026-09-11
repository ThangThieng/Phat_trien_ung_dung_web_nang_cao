using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>FR-CAT-001 – danh sách danh mục, cache Redis key "categories:all" TTL 60 phút.</summary>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>, ICacheable
{
    public string CacheKey => CategoryCacheKeys.All;

    public TimeSpan Expiration => TimeSpan.FromMinutes(60);
}

public sealed class GetCategoriesQueryHandler(ICategoryReadRepository categories)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) =>
        categories.GetAllWithRecipeCountAsync(cancellationToken);
}
