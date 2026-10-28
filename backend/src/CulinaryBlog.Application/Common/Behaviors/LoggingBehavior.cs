using System.Diagnostics;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Common.Behaviors;

/// <summary>
/// Pipeline #1 (SRS §6.3): log tên request + thời gian xử lý, cảnh báo khi &gt; 500ms (FR-OBS-002).
///
/// Buổi 8 — audit log (NFR-SEC-006 "Audit trail: log mọi write operation với userId + timestamp") nằm NGAY TRONG behavior này,
/// không thêm behavior thứ năm: SRS §6.3 chốt đúng 4 behavior (MT-31 đã bỏ <c>PerformanceBehavior</c> vì cùng lý do — đo thời
/// gian và ghi log là việc tự nhiên của logging).
/// - Command (tên kết thúc bằng "Command" — <c>MediatRRequestNamingTests</c> buộc mọi request theo quy ước này) thành công →
///   Information <c>AUDIT {Command} by {UserId} succeeded</c>; thất bại → Warning kèm mã lỗi Phụ lục B.
/// - Query → Information <c>Handled …</c> (không phải thao tác ghi nên không phải audit).
/// - <b>Không bao giờ ghi payload</b> của request: command đăng nhập/đổi mật khẩu/refresh mang mật khẩu và token (NFR-SEC-007).
///   Chỉ ghi TÊN kiểu request — thời điểm do Serilog gắn vào mọi log event.
/// </summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    private static readonly bool IsCommand = typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        LogHandling(logger, requestName);

        TResponse response;
        try
        {
            response = await next(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsCommand && ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            LogAuditFailed(logger, requestName, ActorId, ErrorCodeOf(ex), stopwatch.ElapsedMilliseconds);
            throw;
        }

        stopwatch.Stop();
        var elapsed = stopwatch.ElapsedMilliseconds;
        if (IsCommand)
        {
            LogAuditSucceeded(logger, requestName, ActorId, elapsed);
        }
        else if (elapsed <= SlowRequestThresholdMs)
        {
            LogHandled(logger, requestName, elapsed);
        }

        if (elapsed > SlowRequestThresholdMs)
        {
            LogSlow(logger, requestName, elapsed);
        }

        return response;
    }

    /// <summary>Command chạy khi chưa đăng nhập (đăng ký, đăng nhập, refresh, quên mật khẩu) ghi "anonymous".</summary>
    private string ActorId => currentUser.UserId ?? AuditLog.AnonymousUser;

    /// <summary>Mã lỗi Phụ lục B của exception — cùng giá trị "type" mà GlobalExceptionMiddleware trả cho client.</summary>
    private static string ErrorCodeOf(Exception exception) => exception switch
    {
        DomainException domain => domain.Code,
        AppException app => app.ErrorCode,
        ValidationException => ErrorCodes.ValidationError,
        _ => ErrorCodes.InternalError,
    };

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs} ms")]
    private static partial void LogHandled(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "AUDIT {Command} by {UserId} succeeded in {ElapsedMs} ms")]
    private static partial void LogAuditSucceeded(ILogger logger, string command, string userId, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AUDIT {Command} by {UserId} failed with {ErrorCode} in {ElapsedMs} ms")]
    private static partial void LogAuditFailed(ILogger logger, string command, string userId, string errorCode, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow request {RequestName} took {ElapsedMs} ms (threshold 500 ms)")]
    private static partial void LogSlow(ILogger logger, string requestName, long elapsedMs);
}

/// <summary>Hằng số dùng chung của audit log (FR-OBS-002) — không đặt trong lớp generic để không bị nhân bản theo kiểu.</summary>
public static class AuditLog
{
    public const string AnonymousUser = "anonymous";
}
