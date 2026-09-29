using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Features.Categories;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

/// <summary>SRS §8.2 – /api/v1/categories. GET công khai; POST/PUT/DELETE yêu cầu role Admin (FR-CAT-003/004/005).</summary>
public static class CategoriesEndpoints
{
    public static RouteGroupBuilder MapCategoriesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/categories").WithTags("Categories");

        group.MapGet("/", GetCategoriesAsync)
            .WithName("GetCategories")
            .WithSummary("FR-CAT-001 – Danh sách danh mục (Redis cache 30 phút)")
            .Produces<IReadOnlyList<CategoryDto>>();

        group.MapGet("/{slug}", GetCategoryBySlugAsync)
            .WithName("GetCategoryBySlug")
            .WithSummary("FR-CAT-002 – Chi tiết danh mục + công thức phân trang")
            .Produces<CategoryDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateCategoryAsync)
            .WithName("CreateCategory")
            .WithSummary("FR-CAT-003 – Tạo danh mục, slug tự sinh (Admin)")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateCategoryAsync)
            .WithName("UpdateCategory")
            .WithSummary("FR-CAT-004 – Cập nhật danh mục, slug không đổi (Admin)")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .Produces<CategoryDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteCategoryAsync)
            .WithName("DeleteCategory")
            .WithSummary("FR-CAT-005 – Xóa mềm danh mục, chặn khi còn công thức (Admin)")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return api;
    }

    private static async Task<IResult> GetCategoriesAsync(ISender sender, CancellationToken ct) =>
        Results.Ok(await sender.Send(new GetCategoriesQuery(), ct).ConfigureAwait(false));

    private static async Task<IResult> GetCategoryBySlugAsync(string slug, ISender sender, CancellationToken ct, int page = 1, int pageSize = 12) =>
        Results.Ok(await sender.Send(new GetCategoryBySlugQuery(slug, page, pageSize), ct).ConfigureAwait(false));

    private static async Task<IResult> CreateCategoryAsync(CategoryRequest body, ISender sender, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(body);

        var command = new CreateCategoryCommand(
            body.Name ?? string.Empty,
            body.Description,
            body.ImageUrl,
            body.OrderIndex ?? 0);

        var category = await sender.Send(command, ct).ConfigureAwait(false);
        return Results.Created($"/api/v1/categories/{category.Slug}", category);
    }

    private static async Task<IResult> UpdateCategoryAsync(Guid id, CategoryRequest body, ISender sender, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(body);

        var command = new UpdateCategoryCommand(
            id,
            body.Name ?? string.Empty,
            body.Description,
            body.ImageUrl,
            body.OrderIndex ?? 0);

        return Results.Ok(await sender.Send(command, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> DeleteCategoryAsync(Guid id, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteCategoryCommand(id), ct).ConfigureAwait(false);
        return Results.NoContent();
    }

    /// <summary>Body dùng chung cho POST (FR-CAT-003) và PUT (FR-CAT-004) – SRS §8.2.</summary>
    public sealed record CategoryRequest(string? Name, string? Description, string? ImageUrl, int? OrderIndex);
}
