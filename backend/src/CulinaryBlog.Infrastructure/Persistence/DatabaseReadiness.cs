using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

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

        string connectionString;
        await using (var scope = services.CreateAsyncScope())
        {
            connectionString = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Database.GetConnectionString()
                ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");
        }

        var attempt = 0;
        while (stopwatch.Elapsed < MaxWait)
        {
            attempt++;
            if (await CanOpenConnectionAsync(connectionString, cancellationToken).ConfigureAwait(false))
            {
                LogReady(logger, attempt, (long)stopwatch.Elapsed.TotalSeconds);
                return;
            }

            LogWaiting(logger, attempt);
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt, 5)), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException($"PostgreSQL không sẵn sàng sau {MaxWait.TotalSeconds:0} giây.");
    }

    /// <summary>
    /// Mở kết nối Npgsql trực tiếp, không qua DbContext: CanConnectAsync của EF chạy trong execution strategy
    /// (EnableRetryOnFailure) nên mỗi lần thử lại tự retry thêm 6 lần và ghi cả stack trace ra log.
    /// </summary>
    private static async Task<bool> CanOpenConnectionAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException)
        {
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "PostgreSQL is ready after {Attempts} attempt(s) in {ElapsedSeconds}s")]
    private static partial void LogReady(ILogger logger, int attempts, long elapsedSeconds);

    [LoggerMessage(Level = LogLevel.Warning, Message = "PostgreSQL is not reachable yet (attempt {Attempt}) – waiting before retry")]
    private static partial void LogWaiting(ILogger logger, int attempt);
}
