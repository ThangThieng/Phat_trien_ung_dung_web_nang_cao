using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>FR-RCP-005 – Draft → Published (≥ 1 bước và ≥ 1 nguyên liệu; PublishedAt gán ở lần đầu).</summary>
public sealed record PublishRecipeCommand(Guid RecipeId) : RecipeWriteCommand, IRequest<RecipeDetailDto>;

/// <summary>FR-RCP-005 – Published → Draft.</summary>
public sealed record UnpublishRecipeCommand(Guid RecipeId) : RecipeWriteCommand, IRequest<RecipeDetailDto>;

/// <summary>FR-RCP-006 – Draft hoặc Published → Archived.</summary>
public sealed record ArchiveRecipeCommand(Guid RecipeId) : RecipeWriteCommand, IRequest<RecipeDetailDto>;

/// <summary>FR-RCP-006 – Archived → Draft (MT-35).</summary>
public sealed record UnarchiveRecipeCommand(Guid RecipeId) : RecipeWriteCommand, IRequest<RecipeDetailDto>;

/// <summary>FR-RCP-007 – xóa mềm (không xóa bản ghi con, không xóa tệp MinIO — việc của PermanentPurgeJob sau 30 ngày).</summary>
public sealed record DeleteRecipeCommand(Guid RecipeId) : RecipeWriteCommand, IRequest;

/// <summary>
/// Luồng chung của mọi command vòng đời (Buổi 4 — Dev 4): nạp công thức KÈM Steps + Ingredients + Images (dùng
/// GetByIdAsync thì hai collection rỗng và công thức đủ điều kiện vẫn bị từ chối xuất bản) → 404 nếu không có →
/// 403 RECIPE_FORBIDDEN nếu không phải chủ/Admin → áp hành động của Domain → lưu → khai báo khóa cache cần xóa.
/// Công thức vào/ra tập Published nên xóa cả danh sách, tìm kiếm, sitemap và danh mục (ForVisibilityChange).
/// </summary>
public sealed class RecipeLifecycle(
    IUnitOfWork unitOfWork,
    IAuthorizationService authorization,
    ICurrentUser currentUser)
{
    public async Task<Recipe> ApplyAsync(
        RecipeWriteCommand command,
        Guid recipeId,
        Action<Recipe> action,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(action);

        var recipe = await authorization
            .LoadWithDetailsForWriteAsync(currentUser, unitOfWork, recipeId, cancellationToken)
            .ConfigureAwait(false);

        action(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        command.InvalidateOnSuccess(RecipeCacheKeys.ForVisibilityChange(recipe.Slug));
        return recipe;
    }
}

public sealed class PublishRecipeCommandHandler(RecipeLifecycle lifecycle, IRecipeReadRepository reader, TimeProvider timeProvider)
    : IRequestHandler<PublishRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(PublishRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var recipe = await lifecycle
            .ApplyAsync(request, request.RecipeId, r => r.Publish(timeProvider.GetUtcNow().UtcDateTime), cancellationToken)
            .ConfigureAwait(false);
        return await RecipeLifecycleResult.ReadAsync(reader, recipe.Id, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class UnpublishRecipeCommandHandler(RecipeLifecycle lifecycle, IRecipeReadRepository reader)
    : IRequestHandler<UnpublishRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(UnpublishRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var recipe = await lifecycle.ApplyAsync(request, request.RecipeId, r => r.Unpublish(), cancellationToken).ConfigureAwait(false);
        return await RecipeLifecycleResult.ReadAsync(reader, recipe.Id, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class ArchiveRecipeCommandHandler(RecipeLifecycle lifecycle, IRecipeReadRepository reader)
    : IRequestHandler<ArchiveRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(ArchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var recipe = await lifecycle.ApplyAsync(request, request.RecipeId, r => r.Archive(), cancellationToken).ConfigureAwait(false);
        return await RecipeLifecycleResult.ReadAsync(reader, recipe.Id, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class UnarchiveRecipeCommandHandler(RecipeLifecycle lifecycle, IRecipeReadRepository reader)
    : IRequestHandler<UnarchiveRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(UnarchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var recipe = await lifecycle.ApplyAsync(request, request.RecipeId, r => r.Unarchive(), cancellationToken).ConfigureAwait(false);
        return await RecipeLifecycleResult.ReadAsync(reader, recipe.Id, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class DeleteRecipeCommandHandler(RecipeLifecycle lifecycle) : IRequestHandler<DeleteRecipeCommand>
{
    public Task Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return lifecycle.ApplyAsync(request, request.RecipeId, r => r.SoftDelete(), cancellationToken);
    }
}

internal static class RecipeLifecycleResult
{
    /// <summary>SRS §3.3: response 200 là RecipeDto mới nhất (kèm rowVersion mới cho lần sửa kế tiếp).</summary>
    public static async Task<RecipeDetailDto> ReadAsync(IRecipeReadRepository reader, Guid recipeId, CancellationToken cancellationToken) =>
        await reader.GetDetailByIdAsync(recipeId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Không đọc lại được công thức '{recipeId}' sau khi đổi trạng thái.");
}
