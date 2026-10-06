using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Features.Recipes;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// SRS §8.3 – vòng đời công thức (Buổi 4 — Dev 4): xuất bản, hủy xuất bản, lưu trữ, khôi phục, xóa mềm. Gắn vào CÙNG
/// route group <c>/recipes</c> với <see cref="RecipesEndpoints"/>; tách file theo trách nhiệm để ba dev cùng sửa module
/// Recipe trong một buổi mà không đụng một file. Ghi theo <c>id</c> (đọc theo slug) — SRS §8 quy ước định danh.
/// </summary>
public static class RecipeLifecycleEndpoints
{
    public static RouteGroupBuilder MapRecipeLifecycleEndpoints(this RouteGroupBuilder recipes)
    {
        recipes.MapPatch("/{id:guid}/publish", PublishAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("PublishRecipe")
            .WithSummary("FR-RCP-005 – Xuất bản (Draft → Published; cần ≥ 1 bước và ≥ 1 nguyên liệu)")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        recipes.MapPatch("/{id:guid}/unpublish", UnpublishAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UnpublishRecipe")
            .WithSummary("FR-RCP-005 – Hủy xuất bản (Published → Draft)")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        recipes.MapPatch("/{id:guid}/archive", ArchiveAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("ArchiveRecipe")
            .WithSummary("FR-RCP-006 – Lưu trữ (Draft hoặc Published → Archived)")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        recipes.MapPatch("/{id:guid}/unarchive", UnarchiveAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("UnarchiveRecipe")
            .WithSummary("FR-RCP-006 – Khôi phục từ lưu trữ (Archived → Draft)")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        recipes.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.Author)
            .WithName("DeleteRecipe")
            .WithSummary("FR-RCP-007 – Xóa mềm công thức (dọn vĩnh viễn sau 30 ngày bởi FR-JOB-003)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return recipes;
    }

    private static async Task<IResult> PublishAsync(Guid id, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new PublishRecipeCommand(id), ct).ConfigureAwait(false));

    private static async Task<IResult> UnpublishAsync(Guid id, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new UnpublishRecipeCommand(id), ct).ConfigureAwait(false));

    private static async Task<IResult> ArchiveAsync(Guid id, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new ArchiveRecipeCommand(id), ct).ConfigureAwait(false));

    private static async Task<IResult> UnarchiveAsync(Guid id, ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new UnarchiveRecipeCommand(id), ct).ConfigureAwait(false));

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteRecipeCommand(id), ct).ConfigureAwait(false);
        return Results.NoContent();
    }
}
