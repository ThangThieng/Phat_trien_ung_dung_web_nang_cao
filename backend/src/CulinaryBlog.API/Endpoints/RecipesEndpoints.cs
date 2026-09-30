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
            .Produces<RecipeDetailDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateRecipeAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipe")
            .WithSummary("FR-RCP-004 – Cập nhật công thức (Optimistic Concurrency: If-Match hoặc rowVersion trong body)")
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
            .Produces<RecipeIngredientDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/ingredients/{ingId:guid}", UpdateIngredientAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipeIngredient")
            .WithSummary("FR-RCP-009 – Cập nhật nguyên liệu")
            .Produces<RecipeIngredientDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/ingredients/{ingId:guid}", DeleteIngredientAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeIngredient")
            .WithSummary("FR-RCP-009 – Xóa nguyên liệu")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ---- FR-RCP-010: bước nấu (SRS §8.5) — body KHÔNG có stepNumber ----
        group.MapPost("/{id:guid}/steps", AddStepAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("AddRecipeStep")
            .WithSummary("FR-RCP-010 – Thêm bước (server gán StepNumber = Max + 1)")
            .Produces<RecipeStepDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/steps/reorder", ReorderStepsAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("ReorderRecipeSteps")
            .WithSummary("FR-RCP-010 – Sắp xếp lại bước ({ stepIds: [...] } đầy đủ theo thứ tự mới → 1..N)")
            .Produces<IReadOnlyList<RecipeStepDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/steps/{stepId:guid}", UpdateStepAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipeStep")
            .WithSummary("FR-RCP-010 – Cập nhật nội dung một bước")
            .Produces<RecipeStepDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/steps/{stepId:guid}", DeleteStepAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeStep")
            .WithSummary("FR-RCP-010 – Xóa bước (server đánh số lại các bước còn lại)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/images", UploadImageAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UploadRecipeImage")
            .WithSummary("FR-RCP-008 – Tải ảnh lên cho công thức (multipart: file, altText?, isPrimary?, orderIndex?)")
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
            .Produces<RecipeImageResultDto>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/images/{imageId:guid}", DeleteImageAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeImage")
            .WithSummary("FR-RCP-008 – Xóa ảnh (tệp MinIO xóa bất đồng bộ qua Hangfire)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .WithSummary("FR-RCP-001 – Danh sách công thức (phân trang, lọc, sắp xếp)")
            .CacheOutput(OutputCachePolicies.RecipeList)
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{slug}", GetRecipeBySlugAsync)
            .WithName("GetRecipeBySlug")
            .WithSummary("FR-RCP-002 – Chi tiết công thức theo slug")
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
        var result = await sender.Send(new AddIngredientCommand(id, body), ct).ConfigureAwait(false);
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
        var result = await sender.Send(new AddStepCommand(id, body), ct).ConfigureAwait(false);
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
