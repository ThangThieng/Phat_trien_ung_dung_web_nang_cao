using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.3 – /api/v1/recipes.</summary>
public static class RecipesEndpoints
{
    public static RouteGroupBuilder MapRecipesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/recipes").WithTags("Recipes");

        group.MapPost("/", CreateRecipeAsync).RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("CreateRecipe").WithSummary("FR-RCP-003 – Tạo công thức ở trạng thái Draft")
            .Produces<RecipeDetailDto>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/images", UploadImageAsync).RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UploadRecipeImage").DisableAntiforgery().Accepts<IFormFile>("multipart/form-data")
            .Produces<RecipeImageDto>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        group.MapPatch("/{id:guid}/images/{imageId:guid}", UpdateImageAsync).RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UpdateRecipeImage").Produces<RecipeImageDto>().ProducesProblem(StatusCodes.Status403Forbidden);
        group.MapDelete("/{id:guid}/images/{imageId:guid}", DeleteImageAsync).RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipeImage").Produces(StatusCodes.Status204NoContent);

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

        return api;
    }

    private static async Task<IResult> CreateRecipeAsync(CreateRecipeRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateRecipeCommand(request.Title, request.Description, request.CategoryId, request.PrepTime, request.CookTime, request.Servings, request.Difficulty, request.Instructions, request.Nutrition), ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/{result.Slug}", result);
    }

    private static async Task<IResult> UploadImageAsync(Guid id, IFormFile file, [FromForm] string? altText, ISender sender, CancellationToken ct)
    {
        await using var content = file.OpenReadStream();
        var result = await sender.Send(new UploadRecipeImageCommand(id, content, file.Length, file.ContentType, altText), ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/recipes/{id}/images/{result.Id}", result);
    }

    private static async Task<IResult> UpdateImageAsync(Guid id, Guid imageId, UpdateRecipeImageRequest request, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new UpdateRecipeImageCommand(id, imageId, request.AltText, request.OrderIndex, request.IsPrimary), ct).ConfigureAwait(false));

    private static async Task<IResult> DeleteImageAsync(Guid id, Guid imageId, ISender sender, CancellationToken ct)
    { await sender.Send(new DeleteRecipeImageCommand(id, imageId), ct).ConfigureAwait(false); return Results.NoContent(); }

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

    public sealed record CreateRecipeRequest(string Title, string Description, Guid CategoryId, int PrepTime, int CookTime, int Servings, RecipeDifficulty Difficulty, string? Instructions, NutritionInput? Nutrition);
    public sealed record UpdateRecipeImageRequest(string? AltText, int? OrderIndex, bool? IsPrimary);
}
