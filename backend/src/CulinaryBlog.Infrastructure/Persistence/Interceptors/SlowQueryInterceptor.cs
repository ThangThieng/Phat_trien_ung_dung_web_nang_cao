using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Persistence.Interceptors;

/// <summary>
/// NFR-PERF-004 / FR-OBS-002 (Buổi 8): cảnh báo mỗi câu SQL chạy quá <b>100 ms</b>. Ngưỡng này thuộc về <b>một truy vấn</b> nên nằm
/// ở EF Core command interceptor — khác ngưỡng 500 ms của cả request trong <c>LoggingBehavior</c> (SRS §6.3, MT-31).
///
/// Chỉ ghi <see cref="DbCommand.CommandText"/> — câu SQL có tham số dạng <c>@p0</c>/<c>$1</c>, <b>không</b> ghi giá trị tham số
/// (email, hash mật khẩu, hash token nằm ở đó — NFR-SEC-007). <c>EnableSensitiveDataLogging</c> vẫn tắt ở mọi môi trường
/// (<c>SlowQueryInterceptorTests</c> khẳng định).
/// </summary>
public sealed partial class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger) : DbCommandInterceptor
{
    public const int ThresholdMs = 100;

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Check(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Check(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Check(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        Check(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Check(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        Check(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void Check(DbCommand command, CommandExecutedEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(eventData);

        var elapsedMs = (long)eventData.Duration.TotalMilliseconds;
        if (elapsedMs > ThresholdMs)
        {
            LogSlowQuery(logger, elapsedMs, command.CommandText);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow query took {ElapsedMs} ms (threshold 100 ms): {CommandText}")]
    private static partial void LogSlowQuery(ILogger logger, long elapsedMs, string commandText);
}
