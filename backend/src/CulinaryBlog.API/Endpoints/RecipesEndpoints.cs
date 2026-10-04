using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// SRS §8.3 / §8.4 – /api/v1/recipes: ghi NỘI DUNG (tạo công thức, ảnh; Buổi 4 thêm bước, nguyên liệu, cập nhật).
/// Mọi GET nằm ở <see cref="RecipeQueryEndpoints"/> (Buổi 4 — Dev 3); vòng đời nằm ở <see cref="RecipeLifecycleEndpoints"/>.
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
            .WithSummary("FR-RCP-003 – Tạo công thức ở trạng thái Draft")
            .Produces<RecipeDetailDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

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
            body.Nutrition);

        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/mine/{result.Id}", result);
    }

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

    public sealed record CreateRecipeRequest(
        string? Title,
        string? Description,
        Guid CategoryId,
        int PrepTime,
        int CookTime,
        int Servings,
        RecipeDifficulty Difficulty,
        string? Instructions,
        NutritionInput? Nutrition);

    public sealed record UpdateRecipeImageRequest(string? AltText, bool? IsPrimary, int? OrderIndex);
}
