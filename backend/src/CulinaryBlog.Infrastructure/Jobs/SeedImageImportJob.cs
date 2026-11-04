using System.Net;
using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Files;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// CR-2026-07 / MT-70: nhập ảnh giấy phép mở của dữ liệu mẫu vào MinIO, qua đúng các lớp kiểm tra của tải ảnh lên
/// (<see cref="ImageUploadInspector"/>) rồi xếp hàng FR-JOB-002. Không hotlink: ảnh do hệ thống tự phục vụ thì không đứt link,
/// không lộ người xem cho bên thứ ba và không phải nới CSP. Lỗi mạng chỉ làm công thức tạm thiếu ảnh, không ảnh hưởng khởi động.
/// Có <c>publishAtUtc</c> là công thức Wikibooks: chỉ xuất bản SAU KHI ảnh đã lưu, nên công thức bổ sung đã xuất bản luôn có ảnh
/// (NFR-SEO-001 cần <c>image</c> trong JSON-LD).
/// <para>
/// Tuân thủ quy định của Wikimedia (wikitech Robot_policy, đo thật: 143 job giãn 5 giây, chạy song song khi thử lại, nhận 429 từ
/// khoảng ảnh thứ 50): hàng đợi riêng <see cref="QueueName"/> có đúng MỘT worker nên không bao giờ tải song song; giãn cách đặt
/// TRONG job nên lần thử lại cũng bị giãn; gặp 429 thì chờ theo <c>Retry-After</c>, cả hàng đợi dừng theo. Thử lại của Hangfire
/// cách nhau đủ lâu để qua được đợt bị giới hạn.
/// </para>
/// </summary>
public sealed partial class SeedImageImportJob(
    CulinaryBlogDbContext db,
    IHttpClientFactory httpClientFactory,
    IFileStorageService storage,
    IImageResizeScheduler imageResize,
    ICacheService cache,
    IOptions<SeedOptions> options,
    ILogger<SeedImageImportJob> logger)
{
    public const string HttpClientName = "wikimedia";

    /// <summary>Hàng đợi có đúng một worker (DependencyInjection): Wikimedia cho phép tối đa 2 kết nối đồng thời.</summary>
    public const string QueueName = "seed_images";

    /// <summary>Retry-After dài hơn mức này thì trả job cho Hangfire thử lại sau, không giữ worker quá lâu.</summary>
    private static readonly TimeSpan MaxRateLimitWait = TimeSpan.FromMinutes(10);

    /// <summary>Danh sách cho phép: URL chỉ đến từ dữ liệu nhúng, nhưng job không tải từ host nào khác dù dữ liệu có sai.</summary>
    private static readonly string[] AllowedHosts = ["upload.wikimedia.org", "thumb.wikimedia.org"];

    [Queue(QueueName)]
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = [300, 900, 1800, 3600, 7200])] // NFR-REL-002; giới hạn tần suất kéo dài hàng phút
    public async Task ExecuteAsync(Guid recipeId, DateTime? publishAtUtc, CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken)
            .ConfigureAwait(false);
        if (recipe is null)
        {
            LogRecipeGone(logger, recipeId);
            return;
        }

        var seed = RecipeSeedCatalog.FindBySlug(recipe.Slug);
        var image = RecipeSeedCatalog.FindImage(recipe.Slug);
        if (image is null)
        {
            LogNoImage(logger, recipe.Slug);
            return;
        }

        if (recipe.Images.Count == 0)
        {
            // Công thức do nhóm biên soạn: chỉ gắn ảnh khi chưa ai sửa, vì phải thay Instructions bằng ghi công ảnh (§2.6.1).
            var isLegacy = seed?.Source is null;
            if (isLegacy && (seed is null || recipe.Instructions != RecipeSeedCatalog.InstructionsSummary(seed)))
            {
                LogEdited(logger, recipe.Slug);
                return;
            }

            if (!await ImportAsync(recipe, image, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            if (isLegacy)
            {
                db.Entry(recipe).Property(r => r.Instructions).CurrentValue = RecipeSeedCatalog.LegacyImageCredit(image);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (publishAtUtc is { } publishAt && recipe.Status == RecipeStatus.Draft && recipe.PublishedAt is null)
        {
            recipe.Publish(publishAt);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await cache
                .InvalidateAsync(RecipeCacheKeys.ForVisibilityChange(recipe.Slug, RecipeStatus.Draft, RecipeStatus.Published), cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await cache.InvalidateAsync(RecipeCacheKeys.ForContentChange(recipe.Slug, recipe.Status), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <returns>False khi dữ liệu nguồn không dùng được: thử lại cũng không khác nên dừng, công thức giữ nguyên không ảnh.</returns>
    private async Task<bool> ImportAsync(Recipe recipe, SeedImage image, CancellationToken cancellationToken)
    {
        if (!AllowedHosts.Contains(image.Url.Host, StringComparer.OrdinalIgnoreCase))
        {
            LogHostRejected(logger, recipe.Slug, image.Url.Host);
            return false;
        }

        using var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await DownloadAsync(client, image.Url, recipe.Slug, cancellationToken).ConfigureAwait(false);

        // Lỗi còn lại (429 lần hai, 5xx) ném ra để Hangfire thử lại theo DelaysInSeconds.
        response.EnsureSuccessStatusCode();

        using var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;

        InspectedImage inspected;
        try
        {
            inspected = await ImageUploadInspector
                .InspectAsync(buffer, buffer.Length, response.Content.Headers.ContentType?.MediaType, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (BadRequestException exception)
        {
            LogInvalidImage(logger, recipe.Slug, exception);
            return false;
        }

        StoredFile stored;
        await using (inspected.Content.ConfigureAwait(false))
        {
            stored = await storage
                .UploadAsync(inspected.Content, $"recipes/{recipe.Id}", inspected.Format.Extension, inspected.Format.ContentType, cancellationToken)
                .ConfigureAwait(false);
        }

        RecipeImage created;
        try
        {
            created = recipe.AddImage(stored.Url, recipe.Title, isPrimary: true);
            db.Add(created);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await storage.DeleteAsync(stored.Url, cancellationToken).ConfigureAwait(false);
            throw;
        }

        // Chỉ xếp hàng sau khi bản ghi đã lưu: job đổi kích thước cần đọc được bản ghi đó.
        imageResize.ScheduleResize(created.Id);
        LogImported(logger, recipe.Slug);
        return true;
    }

    /// <summary>Giãn cách trước mỗi lần tải; gặp 429 thì chờ đúng Retry-After rồi thử lại một lần.</summary>
    private async Task<HttpResponseMessage> DownloadAsync(HttpClient client, Uri url, string slug, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        await Task.Delay(TimeSpan.FromSeconds(settings.ImageImportPaceSeconds), cancellationToken).ConfigureAwait(false);

        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return response;
        }

        var retryAfter = response.Headers.RetryAfter;
        var wait = retryAfter?.Delta
            ?? (retryAfter?.Date is { } date ? date - DateTimeOffset.UtcNow : TimeSpan.FromSeconds(settings.ImageImportRateLimitWaitSeconds));
        if (wait > MaxRateLimitWait)
        {
            return response;
        }

        response.Dispose();
        LogRateLimited(logger, slug, wait);
        if (wait > TimeSpan.Zero)
        {
            await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
        }

        return await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Seed image download rate limited for recipe {Slug}, waiting {Wait}")]
    private static partial void LogRateLimited(ILogger logger, string slug, TimeSpan wait);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed image import skipped: recipe {RecipeId} no longer exists")]
    private static partial void LogRecipeGone(ILogger logger, Guid recipeId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Seed image import skipped: no image for slug {Slug}")]
    private static partial void LogNoImage(ILogger logger, string slug);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed image import skipped: recipe {Slug} was edited by a user")]
    private static partial void LogEdited(ILogger logger, string slug);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Seed image import rejected host {Host} for recipe {Slug}")]
    private static partial void LogHostRejected(ILogger logger, string slug, string host);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Seed image import rejected the downloaded file for recipe {Slug}")]
    private static partial void LogInvalidImage(ILogger logger, string slug, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed image imported for recipe {Slug}")]
    private static partial void LogImported(ILogger logger, string slug);
}

/// <summary>Lên lịch nhập ảnh; tách interface để test seeder không phụ thuộc Hangfire thật.</summary>
public interface ISeedImageImportScheduler
{
    void Enqueue(Guid recipeId, DateTime? publishAtUtc);
}

/// <summary>Chỉ xếp hàng: giãn cách và tải tuần tự do job và hàng đợi một worker đảm nhiệm.</summary>
public sealed class HangfireSeedImageImportScheduler(IBackgroundJobClient jobs) : ISeedImageImportScheduler
{
    public void Enqueue(Guid recipeId, DateTime? publishAtUtc) =>
        jobs.Enqueue<SeedImageImportJob>(job => job.ExecuteAsync(recipeId, publishAtUtc, CancellationToken.None));
}
