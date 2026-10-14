using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// SRS §8.3 / §8.4 – /api/v1/recipes: ghi NỘI DUNG (tạo công thức, ảnh; Buổi 4 thêm bước, nguyên liệu, cập nhật) +
/// hai GET công khai (Buổi 4 Dev 3 chuyển sang RecipeQueryEndpoints). Vòng đời nằm ở <see cref="RecipeLifecycleEndpoints"/>.
/// </summary>
public static class RecipesEndpoints
{
    /// <summary>Route group dùng chung cho mọi file endpoint của module Recipe.</summary>
    public static RouteGroupBuilder MapRecipesGroup(this RouteGroupBuilder api)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.MapGroup("/recipes").WithTags("Recipes");
    }

    public static RouteGroupBuilder MapRecipesEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateRecipeAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("CreateRecipe")
            .WithSummary("FR-RCP-003 – Tạo công thức ở trạng thái Draft (kèm steps?/ingredients? tùy chọn)")
            .WithDescription("FR-RCP-003 — SRS §8.3. Tạo bản nháp, có thể kèm nguyên liệu và bước nấu.")
            .Produces<RecipeDetailDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateRecipeAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipe")
            .WithSummary("FR-RCP-004 – Cập nhật công thức (Optimistic Concurrency: If-Match hoặc rowVersion trong body)")
            .WithDescription("FR-RCP-004 — SRS §8.3. Cập nhật bằng optimistic concurrency và rowVersion.")
            .Produces<RecipeDetailDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // ---- FR-RCP-009: nguyên liệu (SRS §8.6) ----
        group.MapPost("/{id:guid}/ingredients", AddIngredientAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("AddRecipeIngredient")
            .WithSummary("FR-RCP-009 – Thêm nguyên liệu (quantity số và/hoặc quantityText nguyên văn)")
            .WithDescription("FR-RCP-009 — SRS §8.6. Thêm nguyên liệu vào aggregate Recipe.")
            .Produces<RecipeIngredientDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/ingredients/{ingId:guid}", UpdateIngredientAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipeIngredient")
            .WithSummary("FR-RCP-009 – Cập nhật nguyên liệu")
            .WithDescription("FR-RCP-009 — SRS §8.6. Cập nhật định lượng và metadata nguyên liệu.")
            .Produces<RecipeIngredientDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/ingredients/{ingId:guid}", DeleteIngredientAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeIngredient")
            .WithSummary("FR-RCP-009 – Xóa nguyên liệu")
            .WithDescription("FR-RCP-009 — SRS §8.6. Xóa nguyên liệu khỏi công thức.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ---- FR-RCP-010: bước nấu (SRS §8.5) — body KHÔNG có stepNumber ----
        group.MapPost("/{id:guid}/steps", AddStepAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("AddRecipeStep")
            .WithSummary("FR-RCP-010 – Thêm bước (server gán StepNumber = Max + 1)")
            .WithDescription("FR-RCP-010 — SRS §8.5. Server tự cấp stepNumber liên tục.")
            .Produces<RecipeStepDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/steps/reorder", ReorderStepsAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("ReorderRecipeSteps")
            .WithSummary("FR-RCP-010 – Sắp xếp lại bước ({ stepIds: [...] } đầy đủ theo thứ tự mới → 1..N)")
            .WithDescription("FR-RCP-010 — SRS §8.5. Đánh số lại thứ tự trong một transaction.")
            .Produces<IReadOnlyList<RecipeStepDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/steps/{stepId:guid}", UpdateStepAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipeStep")
            .WithSummary("FR-RCP-010 – Cập nhật nội dung một bước")
            .WithDescription("FR-RCP-010 — SRS §8.5. Cập nhật nội dung, timer và ảnh của bước.")
            .Produces<RecipeStepDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/steps/{stepId:guid}", DeleteStepAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeStep")
            .WithSummary("FR-RCP-010 – Xóa bước (server đánh số lại các bước còn lại)")
            .WithDescription("FR-RCP-010 — SRS §8.5. Xóa bước và đánh số lại các bước còn lại.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/images", UploadImageAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UploadRecipeImage")
            .WithSummary("FR-RCP-008 – Tải ảnh lên cho công thức (multipart: file, altText?, isPrimary?, orderIndex?)")
            .WithDescription("FR-RCP-008 — SRS §8.4. Lưu ảnh gốc và xếp hàng tạo các kích thước nhỏ.")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<RecipeImageResultDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapPatch("/{id:guid}/images/{imageId:guid}", UpdateImageAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipeImage")
            .WithSummary("FR-RCP-008 – Cập nhật metadata ảnh (altText, isPrimary, orderIndex)")
            .WithDescription("FR-RCP-008 — SRS §8.4. Cập nhật metadata và bảo đảm chỉ một ảnh chính.")
            .Produces<RecipeImageResultDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/images/{imageId:guid}", DeleteImageAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeImage")
            .WithSummary("FR-RCP-008 – Xóa ảnh (tệp MinIO xóa bất đồng bộ qua Hangfire)")
            .WithDescription("FR-RCP-008 — SRS §8.4. Xóa metadata và dọn object bằng background job.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .WithSummary("FR-RCP-001 – Danh sách công thức (phân trang, lọc, sắp xếp)")
            .WithDescription("FR-RCP-001 — SRS §8.3. Trả công thức Published theo bộ lọc và phân trang.")
            .CacheOutput(OutputCachePolicies.RecipeList)
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/mine", GetMyRecipesAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("GetMyRecipes")
            .WithSummary("FR-RCP-011 – Danh sách công thức riêng tư của tôi")
            .WithDescription("FR-RCP-011 — SRS §8.3. Trả Draft, Published, Archived của chính Author; không cache.")
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/search", SearchRecipesAsync)
            .WithName("SearchRecipes")
            .WithSummary("FR-SRCH-001 – Tìm kiếm toàn văn công thức Published")
            .WithDescription("FR-SRCH-001 — SRS §8.3. Tìm tiếng Việt không dấu, xếp theo ts_rank và phân trang.")
            .Produces<PagedResult<RecipeSearchResultDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/sitemap", GetRecipeSitemapAsync)
            .WithName("GetRecipeSitemap")
            .WithSummary("NFR-SEO-003 – Danh sách slug Published cho sitemap")
            .WithDescription("NFR-SEO-003 — SRS §8.3. Trả toàn bộ slug và updatedAt, cache Redis một giờ.")
            .Produces<IReadOnlyList<RecipeSitemapEntryDto>>();

        // Endpoint riêng tư: Draft/Archived tuyệt đối không đi qua GET /{slug} công khai và cache chung.
        group.MapGet("/mine/{id:guid}", GetMyRecipeAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("GetMyRecipe")
            .WithSummary("FR-RCP-004 – Đọc riêng tư một công thức để chỉnh sửa (no-store)")
            .WithDescription("CR-2026-04 d — SRS §8.3. Owner/Admin đọc mọi trạng thái cùng rowVersion.")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{slug}", GetRecipeBySlugAsync)
            .WithName("GetRecipeBySlug")
            .WithSummary("FR-RCP-002 – Chi tiết công thức theo slug")
            .WithDescription("FR-RCP-002 — SRS §8.3. Chi tiết công thức công khai theo slug.")
            .CacheOutput(OutputCachePolicies.RecipeDetail)
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    /// <summary>Location trỏ về tiền tố riêng tư /mine/{id}: công thức mới là Draft nên /recipes/{slug} công khai không thấy nó (MT-34).</summary>
    private static async Task<IResult> CreateRecipeAsync(CreateRecipeRequest body, ISender sender, CancellationToken ct)
    {
        var command = new CreateRecipeCommand(
            body.Title ?? string.Empty,
            body.Description ?? string.Empty,
            body.CategoryId,
            body.PrepTime,
            body.CookTime,
            body.Servings,
            body.Difficulty,
            body.Instructions,
            body.Nutrition,
            body.Steps,
            body.Ingredients);

        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/mine/{result.Id}", result);
    }

    /// <summary>
    /// FR-RCP-004: rowVersion lấy từ header <c>If-Match</c> (chuẩn ETag) nếu có, ngược lại từ body. Response mang
    /// <c>ETag</c> = rowVersion mới để client dùng cho lần sửa kế tiếp.
    /// </summary>
    private static async Task<IResult> UpdateRecipeAsync(Guid id, UpdateRecipeRequest body, HttpContext http, ISender sender, CancellationToken ct)
    {
        var rowVersion = ETags.Parse(http.Request.Headers.IfMatch.ToString()) ?? body.RowVersion;
        var command = new UpdateRecipeCommand(
            id,
            body.Title,
            body.Description,
            body.CategoryId,
            body.PrepTime,
            body.CookTime,
            body.Servings,
            body.Difficulty,
            body.Instructions,
            body.Nutrition,
            rowVersion);

        var result = await sender.Send(command, ct).ConfigureAwait(false);
        http.Response.Headers.ETag = ETags.Format(result.RowVersion);
        return Results.Ok(result);
    }

    private static async Task<IResult> AddIngredientAsync(Guid id, IngredientInput body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddIngredientCommand(id, body.Name, body.Quantity, body.QuantityText, body.Unit, body.Notes, body.OrderIndex), ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/{id}/ingredients/{result.Id}", result);
    }

    private static async Task<IResult> UpdateIngredientAsync(Guid id, Guid ingId, IngredientInput body, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(
            new UpdateIngredientCommand(id, ingId, body.Name, body.Quantity, body.QuantityText, body.Unit, body.Notes, body.OrderIndex),
            ct).ConfigureAwait(false));

    private static async Task<IResult> DeleteIngredientAsync(Guid id, Guid ingId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteIngredientCommand(id, ingId), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<IResult> AddStepAsync(Guid id, StepInput body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddStepCommand(id, body.Title, body.Description, body.TimerMinutes, body.ImageUrl), ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/{id}/steps/{result.Id}", result);
    }

    private static async Task<IResult> UpdateStepAsync(Guid id, Guid stepId, StepInput body, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(
            new UpdateStepCommand(id, stepId, body.Title, body.Description, body.TimerMinutes, body.ImageUrl),
            ct).ConfigureAwait(false));

    private static async Task<IResult> DeleteStepAsync(Guid id, Guid stepId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteStepCommand(id, stepId), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<IResult> ReorderStepsAsync(Guid id, ReorderStepsRequest body, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new ReorderStepsCommand(id, body.StepIds ?? []), ct).ConfigureAwait(false));

    private static async Task<IResult> UploadImageAsync(
        Guid id,
        IFormFile file,
        [FromForm] string? altText,
        [FromForm] bool? isPrimary,
        [FromForm] int? orderIndex,
        ISender sender,
        CancellationToken ct)
    {
        await using var content = file.OpenReadStream();
        var command = new UploadRecipeImageCommand(id, content, file.Length, file.ContentType, altText, isPrimary, orderIndex);
        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/{id}/images/{result.ImageId}", result);
    }

    private static async Task<IResult> UpdateImageAsync(Guid id, Guid imageId, UpdateRecipeImageRequest body, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new UpdateRecipeImageCommand(id, imageId, body.AltText, body.IsPrimary, body.OrderIndex), ct).ConfigureAwait(false));

    private static async Task<IResult> DeleteImageAsync(Guid id, Guid imageId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteRecipeImageCommand(id, imageId), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    private static async Task<IResult> GetRecipesAsync(
        ISender sender,
        CancellationToken ct,
        int page = 1,
        int pageSize = 12,
        Guid? categoryId = null,
        RecipeDifficulty? difficulty = null,
        int? maxCookTime = null,
        string? sort = null) =>
        Results.Ok(await sender.Send(new GetRecipesQuery(page, pageSize, categoryId, difficulty, maxCookTime, sort), ct).ConfigureAwait(false));

    private static async Task<IResult> GetRecipeBySlugAsync(string slug, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetRecipeBySlugQuery(slug), ct).ConfigureAwait(false));

    private static async Task<IResult> GetMyRecipesAsync(
        HttpContext http,
        ISender sender,
        CancellationToken ct,
        RecipeStatus? status = null,
        int page = 1,
        int pageSize = 12,
        string? sort = null,
        string? sortBy = null,
        string? sortOrder = null,
        Guid? authorId = null)
    {
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(await sender.Send(new GetMyRecipesQuery(status, page, pageSize, sort, sortBy, sortOrder, authorId), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> SearchRecipesAsync(
        ISender sender,
        CancellationToken ct,
        string? q = null,
        int page = 1,
        int pageSize = 12,
        Guid? categoryId = null,
        RecipeDifficulty? difficulty = null) =>
        Results.Ok(await sender.Send(new SearchRecipesQuery(q ?? string.Empty, page, pageSize, categoryId, difficulty), ct).ConfigureAwait(false));

    private static async Task<IResult> GetRecipeSitemapAsync(ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetRecipeSitemapQuery(), ct).ConfigureAwait(false));

    private static async Task<IResult> GetMyRecipeAsync(Guid id, HttpContext http, ISender sender, CancellationToken ct)
    {
        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(await sender.Send(new GetMyRecipeByIdQuery(id), ct).ConfigureAwait(false));
    }

    public sealed record CreateRecipeRequest(
        string? Title,
        string? Description,
        Guid CategoryId,
        int PrepTime,
        int CookTime,
        int Servings,
        RecipeDifficulty Difficulty,
        string? Instructions,
        NutritionInput? Nutrition,
        IReadOnlyList<StepInput>? Steps,
        IReadOnlyList<IngredientInput>? Ingredients);

    public sealed record UpdateRecipeRequest(
        string? Title,
        string? Description,
        Guid? CategoryId,
        int? PrepTime,
        int? CookTime,
        int? Servings,
        RecipeDifficulty? Difficulty,
        string? Instructions,
        NutritionInput? Nutrition,
        string? RowVersion);

    public sealed record ReorderStepsRequest(IReadOnlyList<Guid>? StepIds);

    public sealed record UpdateRecipeImageRequest(string? AltText, bool? IsPrimary, int? OrderIndex);
}
