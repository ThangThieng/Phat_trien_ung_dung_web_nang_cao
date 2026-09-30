using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// SRS §8.3 — các endpoint <c>GET</c> công khai của <c>/recipes</c> (Buổi 4 — Dev 3, gom từ <c>RecipesEndpoints</c>), gắn vào CÙNG
/// route group với <see cref="RecipesEndpoints"/> và <see cref="RecipeLifecycleEndpoints"/>. Theo nguyên tắc Public/Private (MT-34):
/// chỉ trả Published cho mọi người gọi, không đọc danh tính, được cache Redis dùng chung. Các GET còn lại của Buổi 4
/// (<c>/search</c>, <c>/mine</c>, <c>/mine/{id}</c>, <c>/sitemap</c>) sẽ được thêm vào file này.
/// </summary>
public static class RecipeQueryEndpoints
{
    public static RouteGroupBuilder MapRecipeQueryEndpoints(this RouteGroupBuilder recipes)
    {
        recipes.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .WithSummary("FR-RCP-001 – Danh sách công thức Published (phân trang, lọc, sắp xếp sortBy/sortOrder)")
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        recipes.MapGet("/{slug}", GetRecipeBySlugAsync)
            .WithName("GetRecipeBySlug")
            .WithSummary("FR-RCP-002 – Chi tiết công thức Published theo slug (Draft/Archived → 404)")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return recipes;
    }

    /// <summary>
    /// Tham số <c>sort</c> cũ (<c>sort=-field</c>, MT-01) không được khai báo trên endpoint — đọc thẳng từ query string để chuyển
    /// cho validator trả 400 thay vì âm thầm bỏ qua, mà không đưa lại tham số đã loại bỏ vào tài liệu Scalar.
    /// </summary>
    private static async Task<IResult> GetRecipesAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken ct,
        int page = 1,
        int pageSize = RecipeQueryRules.DefaultPageSize,
        Guid? categoryId = null,
        RecipeDifficulty? difficulty = null,
        int? maxCookTime = null,
        int? maxPrepTime = null,
        int? minServings = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        var legacySort = request.Query.TryGetValue("sort", out var sort) ? sort.ToString() : null;
        var query = new GetRecipesQuery(page, pageSize, categoryId, difficulty, maxCookTime, maxPrepTime, minServings, sortBy, sortOrder, legacySort);
        return Results.Ok(await sender.Send(query, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> GetRecipeBySlugAsync(string slug, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetRecipeBySlugQuery(slug), ct).ConfigureAwait(false));
}
