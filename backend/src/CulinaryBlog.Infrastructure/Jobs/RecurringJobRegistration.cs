using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// Đăng ký các recurring job (SRS §3.6) vào storage Hangfire dùng chung. <c>AddOrUpdate</c> idempotent nên gọi ở mọi lần
/// khởi động (cả container api lẫn worker) đều an toàn — lịch chạy chỉ tồn tại một bản trong storage.
/// </summary>
public static class RecurringJobRegistration
{
    public static void RegisterRecurringJobs(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var recurringJobs = services.GetRequiredService<IRecurringJobManager>();

        // FR-JOB-003: 03:30 UTC hằng ngày.
        recurringJobs.AddOrUpdate<PermanentPurgeJob>(
            PermanentPurgeJob.RecurringJobId,
            job => job.ExecuteAsync(CancellationToken.None),
            PermanentPurgeJob.CronExpression,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}
