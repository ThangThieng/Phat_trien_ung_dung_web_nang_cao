using System.Collections.Concurrent;
using System.Globalization;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// Buổi 8 (FR-OBS-002, NFR-SEC-007): sink Serilog trong bộ nhớ. Factory đăng ký nó là <see cref="ILogEventSink"/> trong DI và
/// <c>AddCulinarySerilog</c> đọc sink từ DI (<c>ReadFrom.Services</c>) — test nhận ĐÚNG những log event mà Console/File/Seq nhận,
/// sau cả bước che dữ liệu nhạy cảm.
/// </summary>
public sealed class InMemoryLogSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    public IReadOnlyCollection<LogEvent> Events => [.. _events];

    public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);

    public void Clear() => _events.Clear();

    /// <summary>Log event có thuộc tính <c>CorrelationId</c> bằng <paramref name="correlationId"/>.</summary>
    public IReadOnlyList<LogEvent> ForCorrelation(string correlationId) =>
        [.. Events.Where(e => e.Properties.TryGetValue("CorrelationId", out var value) && Scalar(value) == correlationId)];

    /// <summary>Log event dạng JSON y như sink Console/File ghi ra (CompactJsonFormatter) — kiểm "không chứa chuỗi bí mật".</summary>
    public static string ToJson(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        new CompactJsonFormatter().Format(logEvent, writer);
        return writer.ToString();
    }

    public static string? Scalar(LogEventPropertyValue? value) =>
        value is ScalarValue { Value: var raw } ? Convert.ToString(raw, CultureInfo.InvariantCulture) : null;
}
