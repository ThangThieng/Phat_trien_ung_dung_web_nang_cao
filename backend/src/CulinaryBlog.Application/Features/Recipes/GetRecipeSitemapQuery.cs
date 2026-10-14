using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>NFR-SEO-003: một lần đọc toàn bộ slug Published, cache Redis một giờ.</summary>
public sealed record GetRecipeSitemapQuery : IRequest<IReadOnlyList<RecipeSitemapEntryDto>>, ICacheable
{
    public string CacheKey => RecipeCacheKeys.Sitemap;

    public TimeSpan Expiration => TimeSpan.FromHours(1);
}

public sealed class GetRecipeSitemapQueryHandler(IRecipeReadRepository recipes)
    : IRequestHandler<GetRecipeSitemapQuery, IReadOnlyList<RecipeSitemapEntryDto>>
{
    public Task<IReadOnlyList<RecipeSitemapEntryDto>> Handle(GetRecipeSitemapQuery request, CancellationToken cancellationToken) =>
        recipes.GetPublishedSitemapAsync(cancellationToken);
}
