using CulinaryBlog.Infrastructure.Logging;
using Hangfire.Client;
using Hangfire.Server;
using Serilog.Context;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// FR-OBS-002 (Buổi 8): CorrelationId đi từ request HTTP sang job Hangfire mà request đó sinh ra.
/// - <b>Client (container api), lúc enqueue:</b> ghi <see cref="CorrelationContext.Current"/> vào tham số job <c>CorrelationId</c>
///   (lưu cùng job trong schema <c>hangfire</c>; chỉ là mã tra cứu, không phải bí mật — NFR-SEC-007 cấm token, không cấm mã này).
/// - <b>Server (container hangfire), lúc chạy:</b> đọc lại tham số, đặt <see cref="CorrelationContext"/> và đẩy vào
///   <see cref="LogContext"/> → mọi log của <c>WelcomeEmailJob</c>, <c>ImageResizeJob</c>, <c>DeleteStoredFilesJob</c>,
///   <c>PermanentPurgeJob</c>… mang cùng CorrelationId với request gốc; tìm một mã trên Seq ra cả chuỗi request → job.
/// Job không có tham số (recurring job như purge) nhận mã mới để log của một lượt chạy vẫn nối được với nhau.
///
/// Job chạy đồng bộ trên luồng worker giữa <see cref="OnPerforming"/> và <see cref="OnPerformed"/> (Hangfire chờ cả job async),
/// nên giá trị AsyncLocal đặt ở <see cref="OnPerforming"/> có hiệu lực suốt job và được trả lại ở <see cref="OnPerformed"/>.
/// </summary>
public sealed class CorrelationIdJobFilter : IClientFilter, IServerFilter
{
    private const string ScopeItemKey = "CulinaryBlog.CorrelationScope";

    public void OnCreating(CreatingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = CorrelationContext.Current;
        if (CorrelationContext.IsValid(correlationId))
        {
            context.SetJobParameter(CorrelationContext.PropertyName, correlationId);
        }
    }

    public void OnCreated(CreatedContext context)
    {
        // Không cần làm gì sau khi job đã được lưu.
    }

    public void OnPerforming(PerformingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stored = context.GetJobParameter<string>(CorrelationContext.PropertyName, allowStale: true);
        var correlationId = CorrelationContext.IsValid(stored) ? stored : CorrelationContext.NewId();

        context.Items[ScopeItemKey] = new Scope(
            CorrelationContext.Begin(correlationId),
            LogContext.PushProperty(CorrelationContext.PropertyName, correlationId),
            LogContext.PushProperty("JobId", context.BackgroundJob.Id),
            LogContext.PushProperty("JobType", context.BackgroundJob.Job?.Type.Name));
    }

    public void OnPerformed(PerformedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.TryGetValue(ScopeItemKey, out var scope) && scope is IDisposable disposable)
        {
            disposable.Dispose();
            context.Items.Remove(ScopeItemKey);
        }
    }

    /// <summary>Trả các giá trị theo thứ tự ngược với lúc đặt.</summary>
    private sealed class Scope(params IDisposable[] parts) : IDisposable
    {
        public void Dispose()
        {
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                parts[i].Dispose();
            }
        }
    }
}
