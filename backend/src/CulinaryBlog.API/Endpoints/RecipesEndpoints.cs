using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.3 – /api/v1/recipes.</summary>
public static class RecipesEndpoints
{
    public static RouteGroupBuilder MapRecipesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/recipes").WithTags("Recipes");

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
}
