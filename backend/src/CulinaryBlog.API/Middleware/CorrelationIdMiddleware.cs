using System.Diagnostics;
using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Infrastructure.Logging;
using Serilog.Context;

namespace CulinaryBlog.API.Middleware;

/// <summary>
/// SRS §5.2 / §6.2, FR-OBS-002, CONS-010 (Buổi 8): mỗi request một CorrelationId.
/// - Nhận <c>X-Correlation-ID</c> (Nginx sinh từ <c>$request_id</c> nếu client không gửi; Frontend gửi <c>crypto.randomUUID()</c>) và
///   <b>chỉ chấp nhận ≤ 64 ký tự <c>[A-Za-z0-9-]</c></b>; ngược lại sinh mã mới (chống làm ngập log và chèn dòng log giả).
/// - Trả lại trong response header (CORS đã expose từ Buổi 5) và trong Problem Details (<c>correlationId</c>) → Frontend hiện
///   "Mã tra cứu" (Buổi 6); người dùng đọc mã, dev dán vào Seq ra đúng chuỗi log của request qua mọi tầng và cả job nền.
/// - Đẩy <c>CorrelationId</c> + <c>RequestPath</c> vào <see cref="LogContext"/> cho MỌI log của request; <c>UserId</c> được đẩy sau
///   bước xác thực (<see cref="CorrelationIdMiddlewareExtensions.UseUserLogContext"/>).
/// Đứng ngay sau <c>UseForwardedHeaders</c>, trước request logging, để cả log của rate limit, CORS và lỗi đều mang mã.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>Khóa trong <see cref="HttpContext.Items"/> — Problem Details và request logging đọc mã đã kiểm định dạng ở đây.</summary>
    public const string ItemKey = "CulinaryBlog.CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Nhiều header trùng tên bị StringValues nối bằng dấu phẩy → không hợp lệ → sinh mới.
        var incoming = context.Request.Headers[CorrelationContext.HeaderName].ToString();
        var correlationId = CorrelationContext.IsValid(incoming) ? incoming : CorrelationContext.NewId();

        context.Items[ItemKey] = correlationId;
        context.Request.Headers[CorrelationContext.HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationContext.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // FR-OBS-003: span của request mang cùng mã — tìm trace trên Seq bằng CorrelationId cũng được.
        Activity.Current?.SetTag("correlation_id", correlationId);

        using (CorrelationContext.Begin(correlationId))
        using (LogContext.PushProperty(CorrelationContext.PropertyName, correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    public static string? Get(HttpContext context) =>
        context?.Items.TryGetValue(ItemKey, out var value) == true ? value as string : null;
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();

    /// <summary>
    /// CONS-010: "UserId (khi đã xác thực)" trong mọi log entry. Chỉ biết sau <c>UseAuthentication</c>, nên đây là một bước riêng
    /// ngay sau nó; request ẩn danh không có thuộc tính này (không ghi giá trị giả).
    /// </summary>
    public static IApplicationBuilder UseUserLogContext(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(AppClaimTypes.Subject)?.Value
            : null;
        if (userId is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context).ConfigureAwait(false);
        }
    });
}
