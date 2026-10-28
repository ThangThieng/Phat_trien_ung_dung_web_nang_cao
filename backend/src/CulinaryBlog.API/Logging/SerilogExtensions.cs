using System.Globalization;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Common.Authorization;
using Serilog;
using Serilog.Events;

namespace CulinaryBlog.API.Logging;

/// <summary>
/// FR-OBS-002 / CONS-010 (Buổi 8): Serilog cho CẢ container <c>api</c> lẫn <c>hangfire</c> (cùng image, cùng Program.cs).
/// Sink theo SRS: Console (JSON có cấu trúc — <c>CompactJsonFormatter</c>), File (xoay vòng theo ngày, giữ 14 file) khai báo ở
/// <c>appsettings.json</c>; Seq (development) thêm ở đây khi có <c>Seq:ServerUrl</c>. Mức log: Debug (development) / Information
/// (production), lấy từ <c>Serilog:MinimumLevel</c>.
/// </summary>
public static class SerilogExtensions
{
    private const int SlowRequestThresholdMs = 500;

    public static IServiceCollection AddCulinarySerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName)
    {
        // preserveStaticLogger: mỗi host giữ logger RIÊNG thay vì gán vào biến tĩnh Log.Logger. Mặc định của Serilog cho mọi host trong
        // cùng tiến trình ghi qua một logger toàn cục — host khởi động sau (ví dụ host tối giản của SecurityMiddlewareTests) chiếm mất
        // log của host trước. Production chỉ có một host mỗi tiến trình nên hành vi không đổi.
        services.AddSerilog(Configure, preserveStaticLogger: true);
        return services;

        void Configure(IServiceProvider serviceProvider, LoggerConfiguration logger)
        {
            logger
                .ReadFrom.Configuration(configuration)

                // Sink đăng ký trong DI (ILogEventSink) cũng nhận log — LogRedactionTests dùng một sink trong bộ nhớ.
                .ReadFrom.Services(serviceProvider)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .Destructure.With<SensitiveDataDestructuringPolicy>()

                // Đặt SAU FromLogContext: che cả thuộc tính đến từ LogContext/scope.
                .Enrich.With<SensitivePropertyRedactor>();

            var seqUrl = configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                logger.WriteTo.Seq(seqUrl, formatProvider: CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// Một dòng log cho mỗi request: method, path, status, thời gian xử lý (FR-OBS-002) + UserId, IP thật, CorrelationId, phiên
    /// bản API. Không ghi body hay header nào (header <c>Authorization</c> mang token — NFR-SEC-007). Mức log:
    /// 5xx/exception → Error, &gt; 500 ms → Warning, <c>/health*</c> → Verbose (Docker gọi mỗi 10 giây — không làm ngập log),
    /// còn lại → Information.
    /// </summary>
    public static IApplicationBuilder UseCulinaryRequestLogging(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            // Logger của chính host (AddCulinarySerilog không gán Log.Logger tĩnh); mặc định middleware ghi qua Log.Logger và dòng log
            // hoàn tất request sẽ mất.
            options.Logger = app.ApplicationServices.GetRequiredService<Serilog.ILogger>();
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";
            options.GetLevel = GetLevel;
            options.EnrichDiagnosticContext = (diagnostics, context) =>
            {
                var userId = context.User.Identity?.IsAuthenticated == true
                    ? context.User.FindFirst(AppClaimTypes.Subject)?.Value
                    : null;
                if (userId is not null)
                {
                    diagnostics.Set("UserId", userId);
                }

                diagnostics.Set("ClientIp", context.Connection.RemoteIpAddress?.ToString());
                diagnostics.Set("CorrelationId", CorrelationIdMiddleware.Get(context));

                var apiVersion = context.RequestedApiVersion;
                if (apiVersion is not null)
                {
                    diagnostics.Set("ApiVersion", apiVersion.ToString());
                }
            };
        });

    public static LogEventLevel GetLevel(HttpContext context, double elapsedMs, Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (exception is not null || context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        if (context.Request.Path.StartsWithSegments("/health"))
        {
            return LogEventLevel.Verbose;
        }

        return elapsedMs > SlowRequestThresholdMs ? LogEventLevel.Warning : LogEventLevel.Information;
    }
}
