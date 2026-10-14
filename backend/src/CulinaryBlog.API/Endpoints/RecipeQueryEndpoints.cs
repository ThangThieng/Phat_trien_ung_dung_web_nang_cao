using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// SRS §8.3 — MỌI endpoint <c>GET</c> của <c>/recipes</c> (Buổi 4 — Dev 3), gắn vào CÙNG route group với
/// <see cref="RecipesEndpoints"/> và <see cref="RecipeLifecycleEndpoints"/>. Hai nhóm tách bạch theo nguyên tắc Public/Private
/// (MT-34): công khai (<c>/</c>, <c>/search</c>, <c>/sitemap</c>, <c>/{slug}</c>) chỉ trả Published, không cần đăng nhập, được cache
/// Redis dùng chung; riêng tư (<c>/mine</c>, <c>/mine/{id}</c>) bắt buộc Bearer, KHÔNG cache, trả <c>Cache-Control: no-store</c>.
/// Segment literal (<c>search</c>, <c>mine</c>, <c>sitemap</c>) luôn thắng <c>{slug}</c> bất kể thứ tự khai báo; cả ba nằm trong danh
/// sách slug dành riêng (NFR-SEO-004) nên không công thức nào nhận được slug không truy cập được.
/// </summary>
public static class RecipeQueryEndpoints
{
    private const string NoStore = "no-store";

    public static RouteGroupBuilder MapRecipeQueryEndpoints(this RouteGroupBuilder recipes)
    {
        recipes.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .WithSummary("FR-RCP-001 – Danh sách công thức Published (phân trang, lọc, sắp xếp sortBy/sortOrder)")
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        recipes.MapGet("/mine", GetMyRecipesAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("GetMyRecipes")
            .WithSummary("FR-RCP-011 – Công thức của chính mình ở mọi trạng thái (không cache; authorId chỉ dành cho Admin)")
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        recipes.MapGet("/mine/{id:guid}", GetMyRecipeByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("GetMyRecipeById")
            .WithSummary("CR-2026-04 (d) – Chi tiết một công thức của mình ở mọi trạng thái, kèm rowVersion + ETag (trang sửa)")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        recipes.MapGet("/search", SearchRecipesAsync)
            .WithName("SearchRecipes")
            .WithSummary("FR-SRCH-001 – Tìm kiếm toàn văn tiếng Việt không dấu (chỉ Published, xếp theo ts_rank)")
            .Produces<PagedResult<RecipeSearchResultDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        recipes.MapGet("/sitemap", GetSitemapAsync)
            .WithName("GetRecipeSitemap")
            .WithSummary("MT-48 – { slug, updatedAt } của mọi công thức Published cho sitemap.xml (cache 1 giờ)")
            .Produces<IReadOnlyList<RecipeSitemapEntryDto>>();

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

    private static async Task<IResult> GetMyRecipesAsync(
        HttpResponse response,
        ISender sender,
        CancellationToken ct,
        RecipeStatus? status = null,
        int page = 1,
        int pageSize = RecipeQueryRules.DefaultPageSize,
        string? sortBy = null,
        string? sortOrder = null,
        Guid? authorId = null)
    {
        // Gắn trước khi xử lý: kể cả phản hồi lỗi của endpoint riêng tư cũng không được lưu ở bất kỳ tầng cache nào (NFR-SEC-006).
        response.Headers.CacheControl = NoStore;
        var query = new GetMyRecipesQuery(status, page, pageSize, sortBy, sortOrder, authorId);
        return Results.Ok(await sender.Send(query, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> GetMyRecipeByIdAsync(Guid id, HttpResponse response, ISender sender, CancellationToken ct)
    {
        response.Headers.CacheControl = NoStore;
        var recipe = await sender.Send(new GetMyRecipeByIdQuery(id), ct).ConfigureAwait(false);

        // ETag = "{rowVersion}" (base64 của cột RowVersion) — cùng giá trị PUT /recipes/{id} nhận lại qua If-Match hoặc body (FR-RCP-004).
        response.Headers.ETag = $"\"{recipe.RowVersion}\"";
        return Results.Ok(recipe);
    }

    private static async Task<IResult> SearchRecipesAsync(
        ISender sender,
        CancellationToken ct,
        string? q = null,
        int page = 1,
        int pageSize = RecipeQueryRules.DefaultPageSize,
        Guid? categoryId = null,
        RecipeDifficulty? difficulty = null) =>
        Results.Ok(await sender.Send(new SearchRecipesQuery(q ?? string.Empty, page, pageSize, categoryId, difficulty), ct).ConfigureAwait(false));

    private static async Task<IResult> GetSitemapAsync(ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetRecipeSitemapQuery(), ct).ConfigureAwait(false));

    private static async Task<IResult> GetRecipeBySlugAsync(string slug, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetRecipeBySlugQuery(slug), ct).ConfigureAwait(false));
}
