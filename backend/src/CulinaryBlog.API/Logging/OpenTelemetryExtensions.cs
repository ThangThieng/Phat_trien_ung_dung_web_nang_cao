using System.Diagnostics;
using CulinaryBlog.Application.Common.Diagnostics;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CulinaryBlog.API.Logging;

/// <summary>
/// FR-OBS-003 — Tracing &amp; Metrics (mức <b>C</b>, Buổi 8):
/// - Trace: request ASP.NET Core, HttpClient, truy vấn PostgreSQL (Npgsql — mọi truy vấn của EF Core đi qua đây);
/// - Metric: thời gian request (histogram <c>http.server.request.duration</c> — tách được số request và tỉ lệ lỗi theo
///   <c>http.response.status_code</c>), HttpClient, <c>recipes_created_total</c>, <c>recipes_published_total</c>
///   (<see cref="CulinaryBlogMetrics"/>);
/// - Xuất OTLP/HTTP tới Seq (development: <c>OpenTelemetry:OtlpEndpoint</c> = <c>http://seq:80/ingest/otlp</c>). Production không đặt
///   endpoint thì không xuất, nhưng trace vẫn được tạo để mọi log Serilog mang <c>TraceId</c>/<c>SpanId</c> (log correlation với trace).
/// </summary>
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddCulinaryOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"]?.TrimEnd('/');
        var export = !string.IsNullOrWhiteSpace(otlpEndpoint);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion: "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new DropClientRootSpansSampler())
                    .AddAspNetCoreInstrumentation(options => options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health") && !context.Request.Path.StartsWithSegments("/hangfire"))
                    .AddHttpClientInstrumentation()
                    .AddNpgsql();
                if (export)
                {
                    tracing.AddOtlpExporter(options => UseOtlpHttp(options, $"{otlpEndpoint}/v1/traces"));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(CulinaryBlogMetrics.MeterName);
                if (export)
                {
                    metrics.AddOtlpExporter(options => UseOtlpHttp(options, $"{otlpEndpoint}/v1/metrics"));
                }
            });

        return services;
    }

    /// <summary>Seq nhận OTLP qua HTTP/protobuf; khi đặt Endpoint tường minh, exporter dùng nguyên URL nên phải có đủ đường dẫn tín hiệu.</summary>
    private static void UseOtlpHttp(OtlpExporterOptions options, string endpoint)
    {
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Endpoint = new Uri(endpoint);
    }
}

/// <summary>
/// Giữ mọi trace bắt đầu từ một request (span gốc kiểu Server) và mọi span con của nó; BỎ span gốc kiểu Client — truy vấn
/// polling của worker Hangfire (mỗi giây ở dev), request HTTP của chính sink Seq. Chúng không thuộc request nào của người dùng,
/// giữ lại thì Seq ngập span vô nghĩa.
/// </summary>
public sealed class DropClientRootSpansSampler : Sampler
{
    private readonly ParentBasedSampler _inner = new(new AlwaysOnSampler());

    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        var isRoot = samplingParameters.ParentContext.TraceId == default;
        return isRoot && samplingParameters.Kind == ActivityKind.Client
            ? new SamplingResult(SamplingDecision.Drop)
            : _inner.ShouldSample(samplingParameters);
    }
}
