using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>FR-CAT-001 – danh sách danh mục, cache Redis key "categories:all" TTL 30 phút (bảng TTL chuẩn NFR-PERF-003).</summary>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>, ICacheable
{
    public string CacheKey => CategoryCacheKeys.All;

    // Retrofit D-13 (MT-17): 60 → 30 phút theo bảng TTL chuẩn NFR-PERF-003 – buổi này mới có đường ghi danh mục nên TTL bắt đầu có ý nghĩa thật.
    public TimeSpan Expiration => TimeSpan.FromMinutes(30);
}

public sealed class GetCategoriesQueryHandler(ICategoryReadRepository categories)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) =>
        categories.GetAllWithRecipeCountAsync(cancellationToken);
}
