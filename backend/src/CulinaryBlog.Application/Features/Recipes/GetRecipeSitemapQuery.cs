using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// MT-48 / NFR-SEO-003 — nguồn dữ liệu cho <c>app/sitemap.ts</c> của Next.js: <c>{ slug, updatedAt }</c> của MỌI công thức
/// Published trong một lần, không phân trang (<c>GET /recipes</c> giới hạn <c>pageSize ≤ 50</c> và dữ liệu có thể xê dịch giữa
/// các trang). Cache Redis khóa <c>recipes:sitemap</c> 1 giờ — bằng chu kỳ tái sinh sitemap; các command vòng đời (Dev 4) xóa
/// khóa này khi công thức vào/ra tập Published (<see cref="RecipeCacheKeys.ForVisibilityChange"/>).
/// </summary>
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
