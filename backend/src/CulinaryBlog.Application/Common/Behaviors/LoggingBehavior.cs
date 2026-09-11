using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Common.Behaviors;

/// <summary>Pipeline #1 (SRS §6.3): log tên request + thời gian xử lý, cảnh báo khi &gt; 500ms.</summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        LogHandling(logger, requestName);

        var response = await next(cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            LogSlow(logger, requestName, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            LogHandled(logger, requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs} ms")]
    private static partial void LogHandled(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow request {RequestName} took {ElapsedMs} ms (threshold 500 ms)")]
    private static partial void LogSlow(ILogger logger, string requestName, long elapsedMs);
}
