using CulinaryBlog.Application.Features.Categories;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.2 – /api/v1/categories (public).</summary>
public static class CategoriesEndpoints
{
    public static RouteGroupBuilder MapCategoriesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/categories").WithTags("Categories");

        group.MapGet("/", GetCategoriesAsync)
            .WithName("GetCategories")
            .WithSummary("FR-CAT-001 – Danh sách danh mục (Redis cache 60 phút)")
            .Produces<IReadOnlyList<CategoryDto>>();

        group.MapGet("/{slug}", GetCategoryBySlugAsync)
            .WithName("GetCategoryBySlug")
            .WithSummary("FR-CAT-002 – Chi tiết danh mục + công thức phân trang")
            .Produces<CategoryDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }

    private static async Task<IResult> GetCategoriesAsync(ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetCategoriesQuery(), ct).ConfigureAwait(false));

    private static async Task<IResult> GetCategoryBySlugAsync(string slug, ISender sender, CancellationToken ct, int page = 1, int pageSize = 12) =>
        Results.Ok(await sender.Send(new GetCategoryBySlugQuery(slug, page, pageSize), ct).ConfigureAwait(false));
}
