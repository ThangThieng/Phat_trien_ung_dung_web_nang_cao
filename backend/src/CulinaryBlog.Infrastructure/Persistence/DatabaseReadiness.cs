using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Chờ PostgreSQL nhận kết nối trước khi migrate / khởi động Hangfire.
/// Khi Docker khởi động lại, mọi container chạy song song (bỏ qua depends_on): lúc đó tên "postgres" có thể chưa
/// phân giải được, cổng bị từ chối hoặc server trả 57P03 – các lỗi mà retry của EF không bao quát hết.
/// </summary>
public static partial class DatabaseReadiness
{
    private static readonly TimeSpan MaxWait = TimeSpan.FromMinutes(2);

    public static async Task WaitForDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseReadiness));
        var stopwatch = Stopwatch.StartNew();

        var attempt = 0;
        while (stopwatch.Elapsed < MaxWait)
        {
            attempt++;
            await using (var scope = services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
                if (await db.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false))
                {
                    LogReady(logger, attempt, (long)stopwatch.Elapsed.TotalSeconds);
                    return;
                }
            }

            LogWaiting(logger, attempt);
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt, 5)), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException($"PostgreSQL không sẵn sàng sau {MaxWait.TotalSeconds:0} giây.");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "PostgreSQL is ready after {Attempts} attempt(s) in {ElapsedSeconds}s")]
    private static partial void LogReady(ILogger logger, int attempts, long elapsedSeconds);

    [LoggerMessage(Level = LogLevel.Warning, Message = "PostgreSQL is not reachable yet (attempt {Attempt}) – waiting before retry")]
    private static partial void LogWaiting(ILogger logger, int attempt);
}
