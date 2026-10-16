using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// Xóa tệp trên MinIO ngoài luồng request (FR-RCP-008 bước 16, FR-FILE-002). Idempotent: tệp đã mất không gây lỗi
/// (<see cref="IFileStorageService.DeleteAsync"/> bỏ qua 404) nên thử lại an toàn. MinIO lỗi → Hangfire thử lại 3 lần.
/// </summary>
public sealed partial class DeleteStoredFilesJob(IFileStorageService storage, ILogger<DeleteStoredFilesJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(IReadOnlyCollection<string> fileUrls, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fileUrls);

        foreach (var url in fileUrls)
        {
            await storage.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
        }

        LogDeleted(logger, fileUrls.Count);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleted {FileCount} stored file(s) from object storage")]
    private static partial void LogDeleted(ILogger logger, int fileCount);
}

/// <summary>IFileCleanupScheduler → BackgroundJob.Enqueue.</summary>
public sealed class HangfireFileCleanupScheduler(IBackgroundJobClient jobs) : IFileCleanupScheduler
{
    public void ScheduleDelete(IReadOnlyCollection<string> fileUrls)
    {
        ArgumentNullException.ThrowIfNull(fileUrls);

        if (fileUrls.Count > 0)
        {
            string[] urls = [.. fileUrls];
            jobs.Enqueue<DeleteStoredFilesJob>(job => job.ExecuteAsync(urls, CancellationToken.None));
        }
    }
}
