using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-RCP-004…010: chỉ tác giả sở hữu (<c>AuthorId == sub</c>) hoặc Admin được sửa một công thức. Dùng lại cho TOÀN BỘ
/// command ghi của module Recipe — không handler nào tự viết <c>if (recipe.AuthorId != userId)</c>, nên chỉ có một chỗ
/// để kiểm chứng quy tắc (Buổi 3 — Dev 2).
/// </summary>
public sealed class RecipeAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Recipe>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        Recipe resource)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(resource);

        var userId = context.User.FindFirst(AppClaimTypes.Subject)?.Value;
        if (context.User.IsInRole(Roles.Admin) || (userId is not null && userId == resource.AuthorId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>Bước mở đầu chung của mọi command ghi Recipe: nạp aggregate → 404 nếu không có → 403 nếu không phải chủ/Admin.</summary>
public static class RecipeAccess
{
    public static async Task<Recipe> LoadForWriteAsync(
        this IAuthorizationService authorization,
        ICurrentUser currentUser,
        Func<CancellationToken, Task<Recipe?>> load,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(load);

        var recipe = await load(cancellationToken).ConfigureAwait(false) ?? throw new RecipeNotFoundException(recipeId);
        await authorization
            .EnsureOwnerOrAdminAsync(currentUser, recipe, ErrorCodes.RecipeForbidden, "Bạn không có quyền sửa công thức này.")
            .ConfigureAwait(false);
        return recipe;
    }

    /// <summary>Nạp đủ bước + nguyên liệu + ảnh (nội dung, cập nhật, vòng đời).</summary>
    public static Task<Recipe> LoadWithDetailsForWriteAsync(
        this IAuthorizationService authorization,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        return authorization.LoadForWriteAsync(
            currentUser,
            ct => unitOfWork.Recipes.GetByIdWithDetailsAsync(recipeId, ct),
            recipeId,
            cancellationToken);
    }

    /// <summary>Chỉ nạp ảnh (các command ảnh).</summary>
    public static Task<Recipe> LoadWithImagesForWriteAsync(
        this IAuthorizationService authorization,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        return authorization.LoadForWriteAsync(
            currentUser,
            ct => unitOfWork.Recipes.GetByIdWithImagesAsync(recipeId, ct),
            recipeId,
            cancellationToken);
    }
}
