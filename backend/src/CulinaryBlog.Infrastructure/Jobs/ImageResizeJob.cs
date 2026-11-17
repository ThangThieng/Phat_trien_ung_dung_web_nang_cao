using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// FR-JOB-002 – Image Resize Job (Buổi 4 — Dev 4): từ ảnh gốc sinh <b>medium 800×600</b> và <b>thumbnail 300×300</b>
/// (giữ tỉ lệ, cắt phần giữa), tải lên cùng thư mục <c>recipes/{recipeId}/</c> với ảnh gốc rồi ghi MediumUrl/ThumbnailUrl.
/// Chạy trong container worker <c>hangfire</c> — tác vụ nặng CPU không tranh CPU với API (SRS MT-47).
/// Thất bại hết lượt thử lại thì hai URL giữ nguyên null: FE hiển thị ảnh gốc, việc tải lên không bị ảnh hưởng.
/// Hai biến thể mã hóa WebP: nhỏ hơn JPEG cùng chất lượng và mọi trình duyệt hiện hành đều hỗ trợ.
/// Ghi xong hai URL thì xóa cache công khai của công thức (Buổi 8 — audit cache): job ghi DB ngoài pipeline MediatR nên
/// <c>CacheInvalidationBehavior</c> không chạy; thiếu bước này trang chi tiết/danh sách giữ bản không có thumbnail tới hết TTL.
/// </summary>
public sealed partial class ImageResizeJob(
    CulinaryBlogDbContext db,
    IFileStorageService storage,
    ICacheService cache,
    ILogger<ImageResizeJob> logger)
{
    public static readonly Size MediumSize = new(800, 600);

    public static readonly Size ThumbnailSize = new(300, 300);

    private const string VariantExtension = ".webp";
    private const string VariantContentType = "image/webp";

    private static readonly WebpEncoder Encoder = new() { Quality = 80 };

    /// <summary>
    /// Chỉ nạp decoder của định dạng FR-FILE-001 cho phép mà ImageSharp 3.1 đọc được (JPEG/PNG/WebP; AVIF chưa hỗ trợ).
    /// Ảnh trong kho đã qua kiểm tra magic bytes lúc tải lên; đây là lớp chặn thứ hai: dù object bị thay bằng TIFF,
    /// decoder TIFF/BigTIFF (GHSA-wmxv-xphr-5c9g, chưa vá ở 3.x) không bao giờ chạy — xem Directory.Build.props.
    /// </summary>
    private static readonly DecoderOptions UploadFormatsOnly = new()
    {
        Configuration = new Configuration(new JpegConfigurationModule(), new PngConfigurationModule(), new WebpConfigurationModule()),
    };

    /// <summary>
    /// Định dạng job giải mã được — chỉ JPEG/PNG/WebP. Là bằng chứng cho mọi miễn trừ advisory TIFF trong Directory.Build.props
    /// (BigTIFF, tile nén fax GHSA-v76p-62qx-wwq2…): decoder TIFF không có trong cấu hình nên không đường nào tới được code lỗi.
    /// </summary>
    public static IReadOnlyCollection<string> DecodableFormats { get; } =
        [.. UploadFormatsOnly.Configuration.ImageFormats.Select(f => f.Name).Order(StringComparer.Ordinal)];

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

        await InvalidatePublicCacheAsync(image.RecipeId, cancellationToken).ConfigureAwait(false);
        LogResized(logger, imageId);
    }

    /// <summary>
    /// Cùng bộ khóa với mọi thay đổi nội dung công thức (<see cref="RecipeCacheKeys.ForContentChange"/>): bản nháp chỉ xóa chi tiết,
    /// công thức đã xuất bản xóa cả danh sách và trang danh mục (CR-2026-06).
    /// </summary>
    private async Task InvalidatePublicCacheAsync(Guid recipeId, CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes.AsNoTracking()
            .Where(r => r.Id == recipeId)
            .Select(r => new { r.Slug, r.Status })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (recipe is null)
        {
            return;
        }

        await cache.InvalidateAsync(RecipeCacheKeys.ForContentChange(recipe.Slug, recipe.Status), cancellationToken).ConfigureAwait(false);
    }

    private async Task<(string? MediumUrl, string? ThumbnailUrl)> CreateVariantsAsync(RecipeImage image, CancellationToken cancellationToken)
    {
        Image source;
        var original = await storage.OpenReadAsync(image.OriginalUrl, cancellationToken).ConfigureAwait(false);
        await using (original.ConfigureAwait(false))
        {
            try
            {
                source = await Image.LoadAsync(UploadFormatsOnly, original, cancellationToken).ConfigureAwait(false);
            }
            catch (UnknownImageFormatException ex)
            {
                // AVIF (ImageSharp 3.1 chưa giải mã) hoặc định dạng ngoài danh sách: thử lại cũng không khác → dừng ngay, giữ ảnh gốc (không ném để khỏi retry vô ích).
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
