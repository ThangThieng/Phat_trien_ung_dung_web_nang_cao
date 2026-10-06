using CulinaryBlog.Application.Common.Files;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Recipes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>SRS §8.4 – response ảnh: <c>{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }</c>.</summary>
public sealed record RecipeImageResultDto(
    Guid ImageId,
    string OriginalUrl,
    string? MediumUrl,
    string? ThumbnailUrl,
    string? AltText,
    bool IsPrimary,
    int OrderIndex)
{
    public static RecipeImageResultDto From(RecipeImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return new(image.Id, image.OriginalUrl, image.MediumUrl, image.ThumbnailUrl, image.AltText, image.IsPrimary, image.OrderIndex);
    }
}

/// <summary>SRS §7.9: RecipeImage.AltText ≤ 200; FR-RCP-008 A5: orderIndex không âm.</summary>
internal static class RecipeImageRules
{
    public const int AltTextMax = 200;
}

/// <summary>
/// FR-RCP-008 – tải ảnh lên cho công thức (multipart: <c>file</c>, <c>altText?</c>, <c>isPrimary?</c>, <c>orderIndex?</c>).
/// Tệp lưu ở <c>recipes/{recipeId}/{guid}{ext}</c>; <c>mediumUrl</c>/<c>thumbnailUrl</c> = null cho tới khi FR-JOB-002 chạy.
/// </summary>
public sealed record UploadRecipeImageCommand(
    Guid RecipeId,
    Stream Content,
    long Length,
    string? ContentType,
    string? AltText,
    bool? IsPrimary,
    int? OrderIndex) : RecipeWriteCommand, IRequest<RecipeImageResultDto>;

public sealed class UploadRecipeImageCommandValidator : AbstractValidator<UploadRecipeImageCommand>
{
    public UploadRecipeImageCommandValidator()
    {
        RuleFor(x => x.AltText).MaximumLength(RecipeImageRules.AltTextMax).WithMessage("altText tối đa 200 ký tự.");
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0).When(x => x.OrderIndex.HasValue).WithMessage("orderIndex phải >= 0.");
    }
}

public sealed class UploadRecipeImageCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStorageService storage,
    IFileCleanupScheduler fileCleanup,
    IImageResizeScheduler imageResize,
    IAuthorizationService authorization,
    ICurrentUser currentUser) : IRequestHandler<UploadRecipeImageCommand, RecipeImageResultDto>
{
    public async Task<RecipeImageResultDto> Handle(UploadRecipeImageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 404/403 trước khi đọc file: không tốn băng thông MinIO cho request không có quyền.
        var recipe = await authorization
            .LoadWithImagesForWriteAsync(currentUser, unitOfWork, request.RecipeId, cancellationToken)
            .ConfigureAwait(false);

        var image = await ImageUploadInspector
            .InspectAsync(request.Content, request.Length, request.ContentType, cancellationToken)
            .ConfigureAwait(false);

        StoredFile stored;
        await using (image.Content.ConfigureAwait(false))
        {
            stored = await storage
                .UploadAsync(image.Content, $"recipes/{recipe.Id}", image.Format.Extension, image.Format.ContentType, cancellationToken)
                .ConfigureAwait(false);
        }

        RecipeImage? created = null;
        try
        {
            await unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    var current = await unitOfWork.Recipes.GetByIdWithImagesAsync(request.RecipeId, ct).ConfigureAwait(false)
                        ?? throw new RecipeNotFoundException(request.RecipeId);

                    // Ảnh mới là ảnh chính mà đã có ảnh chính: LƯU bước hạ trước (IDX_RecipeImage_Primary không deferrable).
                    if (request.IsPrimary == true && current.DemoteAllImages())
                    {
                        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                    }

                    created = current.AddImage(stored.Url, request.AltText, request.IsPrimary, request.OrderIndex);
                    await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Tệp đã lên MinIO nhưng bản ghi không lưu được → dọn tệp mồ côi (lỗi rơi về phía "rác thừa", không mất dữ liệu).
            fileCleanup.ScheduleDelete([stored.Url]);
            throw;
        }

        // FR-JOB-002 (Buổi 4 — Dev 4): chỉ xếp hàng SAU KHI bản ghi ảnh đã commit — job cần đọc được bản ghi đó.
        imageResize.ScheduleResize(created!.Id);

        request.InvalidateOnSuccess(RecipeCacheKeys.ForContentChange(recipe.Slug));
        return RecipeImageResultDto.From(created);
    }
}

/// <summary>FR-RCP-008 – PATCH metadata ảnh <c>{ altText?, isPrimary?, orderIndex? }</c>; trường không gửi thì giữ nguyên.</summary>
public sealed record UpdateRecipeImageCommand(Guid RecipeId, Guid ImageId, string? AltText, bool? IsPrimary, int? OrderIndex)
    : RecipeWriteCommand, IRequest<RecipeImageResultDto>;

public sealed class UpdateRecipeImageCommandValidator : AbstractValidator<UpdateRecipeImageCommand>
{
    public UpdateRecipeImageCommandValidator()
    {
        RuleFor(x => x.AltText).MaximumLength(RecipeImageRules.AltTextMax).WithMessage("altText tối đa 200 ký tự.");
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0).When(x => x.OrderIndex.HasValue).WithMessage("orderIndex phải >= 0.");
    }
}

public sealed class UpdateRecipeImageCommandHandler(
    IUnitOfWork unitOfWork,
    IAuthorizationService authorization,
    ICurrentUser currentUser) : IRequestHandler<UpdateRecipeImageCommand, RecipeImageResultDto>
{
    public async Task<RecipeImageResultDto> Handle(UpdateRecipeImageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recipe = await authorization
            .LoadWithImagesForWriteAsync(currentUser, unitOfWork, request.RecipeId, cancellationToken)
            .ConfigureAwait(false);
        _ = recipe.GetImage(request.ImageId); // 404 trước khi mở transaction

        RecipeImage? updated = null;
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var current = await unitOfWork.Recipes.GetByIdWithImagesAsync(request.RecipeId, ct).ConfigureAwait(false)
                    ?? throw new RecipeNotFoundException(request.RecipeId);

                updated = current.UpdateImageMetadata(request.ImageId, request.AltText, request.OrderIndex);

                // isPrimary = true: hạ ảnh chính cũ → LƯU → nâng ảnh mới → LƯU. isPrimary = false bị bỏ qua: công thức có ảnh
                // luôn giữ đúng một ảnh chính, muốn đổi thì chọn ảnh khác làm ảnh chính.
                if (request.IsPrimary == true && !updated.IsPrimary)
                {
                    current.DemoteAllImages();
                    await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                    current.SetPrimaryImage(request.ImageId);
                }

                await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);

        request.InvalidateOnSuccess(RecipeCacheKeys.ForContentChange(recipe.Slug));
        return RecipeImageResultDto.From(updated!);
    }
}

/// <summary>
/// FR-RCP-008 – xóa ảnh: xóa mềm bản ghi, ảnh chính thì ảnh có OrderIndex nhỏ nhất (hòa thì CreatedAt sớm nhất) lên thay,
/// rồi xếp hàng xóa tệp MinIO qua Hangfire (chỉ ở thao tác này — xóa mềm công thức KHÔNG xóa tệp).
/// </summary>
public sealed record DeleteRecipeImageCommand(Guid RecipeId, Guid ImageId) : RecipeWriteCommand, IRequest;

public sealed class DeleteRecipeImageCommandHandler(
    IUnitOfWork unitOfWork,
    IFileCleanupScheduler fileCleanup,
    IAuthorizationService authorization,
    ICurrentUser currentUser) : IRequestHandler<DeleteRecipeImageCommand>
{
    public async Task Handle(DeleteRecipeImageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recipe = await authorization
            .LoadWithImagesForWriteAsync(currentUser, unitOfWork, request.RecipeId, cancellationToken)
            .ConfigureAwait(false);
        _ = recipe.GetImage(request.ImageId);

        RecipeImage? removed = null;
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var current = await unitOfWork.Recipes.GetByIdWithImagesAsync(request.RecipeId, ct).ConfigureAwait(false)
                    ?? throw new RecipeNotFoundException(request.RecipeId);

                removed = current.RemoveImage(request.ImageId);
                await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

                if (current.PromoteFallbackPrimaryImage() is not null)
                {
                    await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                }
            },
            cancellationToken).ConfigureAwait(false);

        // Chỉ xếp hàng xóa tệp SAU KHI DB đã commit: rollback thì tệp vẫn còn nguyên.
        fileCleanup.ScheduleDelete(
            new[] { removed!.OriginalUrl, removed.MediumUrl, removed.ThumbnailUrl }.OfType<string>().ToArray());

        request.InvalidateOnSuccess(RecipeCacheKeys.ForContentChange(recipe.Slug));
    }
}
