using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedLockNet;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// FR-JOB-003 – Permanent Purge Job (Buổi 4 — Dev 4): hằng ngày 03:30 UTC, xóa VĨNH VIỄN công thức đã xóa mềm quá 30 ngày
/// (<c>IsDeleted = true AND UpdatedAt &lt; NOW() - 30 ngày</c>) cùng tệp ảnh của nó. Đây là nơi DUY NHẤT trong hệ thống
/// được xóa vật lý (IRepository&lt;T&gt; cố ý không có xóa cứng — Buổi 3), nên job thao tác thẳng trên DbContext.
///
/// Thứ tự bắt buộc cho mỗi công thức: (1) thu thập mọi URL tệp TRƯỚC khi xóa → (2) xóa cứng trong một transaction (FK
/// ON DELETE CASCADE dọn Steps/Ingredients/Images) → (3) chỉ khi đã commit mới xóa tệp. Làm ngược lại mà DB rollback thì
/// công thức trỏ tới tệp không còn — hỏng vĩnh viễn; làm đúng thứ tự thì trường hợp xấu nhất chỉ là tệp mồ côi, vô hại.
/// </summary>
public sealed partial class PermanentPurgeJob(
    CulinaryBlogDbContext db,
    IFileStorageService storage,
    IFileCleanupScheduler fileCleanup,
    IDistributedLockFactory lockFactory,
    TimeProvider timeProvider,
    ILogger<PermanentPurgeJob> logger)
{
    public const string RecurringJobId = "permanent-purge";

    /// <summary>03:30 UTC hằng ngày (SRS §3.6 FR-JOB-003); đăng ký với <c>TimeZone = UTC</c> để không trôi theo múi giờ của máy chủ.</summary>
    public const string CronExpression = "30 3 * * *";

    public const string LockKey = "lock:purge-job";

    public static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    private static readonly TimeSpan LockExpiry = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Hangfire bảo đảm mỗi job instance chỉ một server nhận, nhưng KHÔNG bảo đảm hai lượt của cùng recurring job không
    /// chồng nhau (thử lại, trigger tay trên dashboard, worker thứ hai khi scale) — nên cần thêm khóa phân tán (NFR-SCALE-001).
    /// Không lấy được khóa thì bỏ lượt này: lượt đang giữ khóa sẽ dọn cùng tập bản ghi.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var distributedLock = await lockFactory.CreateLockAsync(LockKey, LockExpiry).ConfigureAwait(false);
        await using (distributedLock.ConfigureAwait(false))
        {
            if (!distributedLock.IsAcquired)
            {
                LogLockNotAcquired(logger, LockKey);
                return;
            }

            await PurgeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Thân job (không khóa) — tách riêng để integration test gọi trực tiếp và khẳng định kết quả.</summary>
    public async Task<PurgeResult> PurgeAsync(CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().UtcDateTime - RetentionPeriod;

        // IgnoreQueryFilters BẮT BUỘC: Global Query Filter (IsDeleted = false) ẩn chính những bản ghi cần tìm.
        var expiredIds = await db.Recipes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.IsDeleted && r.UpdatedAt != null && r.UpdatedAt < cutoff)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var purgedRecipes = 0;
        var deletedFiles = 0;
        foreach (var recipeId in expiredIds)
        {
            // (1) Thu thập URL trước khi xóa — sau bước (2) các bản ghi ảnh/bước không còn để đọc.
            var fileUrls = await CollectFileUrlsAsync(recipeId, cancellationToken).ConfigureAwait(false);

            // (2) Xóa cứng trong transaction, điều kiện kiểm lại ngay trong câu DELETE: công thức vừa được khôi phục
            //     (IsDeleted = false) giữa bước tìm và bước xóa sẽ không bị xóa nhầm.
            var deleted = await HardDeleteAsync(recipeId, cutoff, cancellationToken).ConfigureAwait(false);
            if (!deleted)
            {
                continue;
            }

            purgedRecipes++;

            // (3) Chỉ sau khi đã commit mới xóa tệp.
            deletedFiles += await DeleteFilesAsync(fileUrls, cancellationToken).ConfigureAwait(false);
        }

        LogPurged(logger, purgedRecipes, deletedFiles, cutoff);
        return new PurgeResult(purgedRecipes, deletedFiles);
    }

    /// <summary>Ảnh gốc + medium + thumbnail của MỌI ảnh (kể cả ảnh đã xóa mềm — idempotent) và ảnh minh họa của từng bước.</summary>
    private async Task<IReadOnlyList<string>> CollectFileUrlsAsync(Guid recipeId, CancellationToken cancellationToken)
    {
        var imageUrls = await db.RecipeImages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(i => i.RecipeId == recipeId)
            .Select(i => new { i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var stepImageUrls = await db.RecipeSteps
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.RecipeId == recipeId && s.ImageUrl != null)
            .Select(s => s.ImageUrl!)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. imageUrls
                .SelectMany(i => new[] { i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl })
                .OfType<string>()
                .Concat(stepImageUrls)
                .Distinct(StringComparer.Ordinal),
        ];
    }

    /// <summary>Transaction mở QUA execution strategy (DbContext bật EnableRetryOnFailure) — cùng mẫu UnitOfWork.ExecuteInTransactionAsync.</summary>
    private Task<bool> HardDeleteAsync(Guid recipeId, DateTime cutoff, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(
            async ct =>
            {
                var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
                await using (transaction.ConfigureAwait(false))
                {
                    var rows = await db.Recipes
                        .IgnoreQueryFilters()
                        .Where(r => r.Id == recipeId && r.IsDeleted && r.UpdatedAt < cutoff)
                        .ExecuteDeleteAsync(ct)
                        .ConfigureAwait(false);

                    await transaction.CommitAsync(ct).ConfigureAwait(false);
                    return rows > 0;
                }
            },
            cancellationToken);
    }

    /// <summary>
    /// Xóa tệp ngay (FR-FILE-002); tệp nào lỗi (MinIO tạm thời không phản hồi) thì chuyển cho DeleteStoredFilesJob thử lại —
    /// bản ghi đã bị xóa nên lượt purge sau sẽ không bao giờ thấy lại các URL này.
    /// </summary>
    private async Task<int> DeleteFilesAsync(IReadOnlyList<string> fileUrls, CancellationToken cancellationToken)
    {
        var deleted = 0;
        var failed = new List<string>();
        foreach (var url in fileUrls)
        {
            try
            {
                await storage.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
                deleted++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogFileDeleteFailed(logger, url, ex);
                failed.Add(url);
            }
        }

        if (failed.Count > 0)
        {
            fileCleanup.ScheduleDelete(failed);
        }

        return deleted;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Permanent purge skipped: distributed lock {LockKey} is held by another run")]
    private static partial void LogLockNotAcquired(ILogger logger, string lockKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Permanent purge finished: {RecipeCount} recipe(s) and {FileCount} file(s) deleted (soft-deleted before {Cutoff:o})")]
    private static partial void LogPurged(ILogger logger, int recipeCount, int fileCount, DateTime cutoff);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Permanent purge could not delete file {FileUrl} – rescheduled")]
    private static partial void LogFileDeleteFailed(ILogger logger, string fileUrl, Exception exception);
}

public sealed record PurgeResult(int PurgedRecipes, int DeletedFiles);
