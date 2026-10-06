using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// FR-JOB-002 – Image Resize Job (Buổi 4 — Dev 4): từ ảnh gốc sinh <b>medium 800×600</b> và <b>thumbnail 300×300</b>
/// (giữ tỉ lệ, cắt phần giữa), tải lên cùng thư mục <c>recipes/{recipeId}/</c> với ảnh gốc rồi ghi MediumUrl/ThumbnailUrl.
/// Chạy trong container worker <c>hangfire</c> — tác vụ nặng CPU không tranh CPU với API (SRS MT-47).
/// Thất bại hết lượt thử lại thì hai URL giữ nguyên null: FE hiển thị ảnh gốc, việc tải lên không bị ảnh hưởng.
/// Hai biến thể mã hóa WebP: nhỏ hơn JPEG cùng chất lượng và mọi trình duyệt hiện hành đều hỗ trợ.
/// </summary>
public sealed partial class ImageResizeJob(
    CulinaryBlogDbContext db,
    IFileStorageService storage,
    ILogger<ImageResizeJob> logger)
{
    public static readonly Size MediumSize = new(800, 600);

    public static readonly Size ThumbnailSize = new(300, 300);

    private const string VariantExtension = ".webp";
    private const string VariantContentType = "image/webp";

    private static readonly WebpEncoder Encoder = new() { Quality = 80 };

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(Guid imageId, CancellationToken cancellationToken)
    {
        var image = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken).ConfigureAwait(false);
        if (image is null)
        {
            // Ảnh (hoặc cả công thức) đã bị xóa trước khi job kịp chạy.
            LogImageGone(logger, imageId);
            return;
        }

        if (image.MediumUrl is not null && image.ThumbnailUrl is not null)
        {
            return; // Idempotent: lượt trước đã xong (Hangfire có thể chạy lại một job đã thành công).
        }

        var (mediumUrl, thumbnailUrl) = await CreateVariantsAsync(image, cancellationToken).ConfigureAwait(false);
        if (mediumUrl is null || thumbnailUrl is null)
        {
            return;
        }

        image.SetResizedVariants(mediumUrl, thumbnailUrl);
        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Người dùng vừa sửa metadata/xóa ảnh cùng lúc: nạp lại rồi thử một lần; ảnh đã bị xóa thì dọn biến thể vừa tải lên.
            db.ChangeTracker.Clear();
            var fresh = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken).ConfigureAwait(false);
            if (fresh is null)
            {
                await storage.DeleteAsync(mediumUrl, cancellationToken).ConfigureAwait(false);
                await storage.DeleteAsync(thumbnailUrl, cancellationToken).ConfigureAwait(false);
                return;
            }

            fresh.SetResizedVariants(mediumUrl, thumbnailUrl);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        LogResized(logger, imageId);
    }

    private async Task<(string? MediumUrl, string? ThumbnailUrl)> CreateVariantsAsync(RecipeImage image, CancellationToken cancellationToken)
    {
        Image source;
        var original = await storage.OpenReadAsync(image.OriginalUrl, cancellationToken).ConfigureAwait(false);
        await using (original.ConfigureAwait(false))
        {
            try
            {
                source = await Image.LoadAsync(original, cancellationToken).ConfigureAwait(false);
            }
            catch (UnknownImageFormatException ex)
            {
                // ImageSharp 3.1 chưa giải mã AVIF: thử lại cũng không khác → dừng ngay, giữ ảnh gốc (không ném để khỏi retry vô ích).
                LogUnsupportedFormat(logger, image.Id, ex);
                return (null, null);
            }
        }

        using (source)
        {
            var folder = $"recipes/{image.RecipeId}";
            var medium = await UploadVariantAsync(source, MediumSize, folder, cancellationToken).ConfigureAwait(false);
            var thumbnail = await UploadVariantAsync(source, ThumbnailSize, folder, cancellationToken).ConfigureAwait(false);
            return (medium, thumbnail);
        }
    }

    private async Task<string> UploadVariantAsync(Image source, Size size, string folder, CancellationToken cancellationToken)
    {
        using var resized = source.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Size = size,
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center,
        }));

        using var output = new MemoryStream();
        await resized.SaveAsync(output, Encoder, cancellationToken).ConfigureAwait(false);
        output.Position = 0;

        var stored = await storage.UploadAsync(output, folder, VariantExtension, VariantContentType, cancellationToken).ConfigureAwait(false);
        return stored.Url;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Image {ImageId} resized to medium 800x600 and thumbnail 300x300")]
    private static partial void LogResized(ILogger logger, Guid imageId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Image resize skipped: image {ImageId} no longer exists")]
    private static partial void LogImageGone(ILogger logger, Guid imageId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Image resize skipped: image {ImageId} has a format ImageSharp cannot decode – original image is kept")]
    private static partial void LogUnsupportedFormat(ILogger logger, Guid imageId, Exception exception);
}

/// <summary>IImageResizeScheduler → BackgroundJob.Enqueue (FR-JOB-002, gọi từ UploadRecipeImageCommand).</summary>
public sealed class HangfireImageResizeScheduler(IBackgroundJobClient jobs) : IImageResizeScheduler
{
    public void ScheduleResize(Guid imageId) =>
        jobs.Enqueue<ImageResizeJob>(job => job.ExecuteAsync(imageId, CancellationToken.None));
}
