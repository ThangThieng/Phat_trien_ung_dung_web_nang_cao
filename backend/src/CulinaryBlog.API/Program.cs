using System.Text.Json.Serialization;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Logging;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.API.Middleware.ExceptionMapping;
using CulinaryBlog.API.OpenApi;
using CulinaryBlog.API.Services;
using CulinaryBlog.Application;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using Hangfire;
using Hangfire.Dashboard;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Container "hangfire" dùng cùng image nhưng chỉ chạy BackgroundJobServer (SRS §6.5)
var workerOnly = builder.Configuration.GetValue<bool>("Hangfire:WorkerOnly");

// FR-OBS-002 / FR-OBS-003 (Buổi 8): Serilog + OpenTelemetry cho CẢ hai container; thuộc tính "Application" tách log của từng loại.
var applicationName = workerOnly ? "CulinaryBlog.Hangfire" : "CulinaryBlog.API";
builder.Services.AddCulinarySerilog(builder.Configuration, applicationName);
builder.Services.AddCulinaryOpenTelemetry(
    builder.Configuration,
    workerOnly ? "culinaryblog-hangfire" : builder.Configuration["OpenTelemetry:ServiceName"] ?? "culinaryblog-api");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Khóa DataProtection lưu bền trên volume dùng chung cho api + hangfire; production mã hóa khóa bằng chứng chỉ (D-17, Buổi 7)
builder.Services.AddCulinaryDataProtection(builder.Configuration, builder.Environment);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTrustedForwardedHeaders(builder.Configuration);

// Buổi 3: bảng "domain exception → mã HTTP" dựng MỘT lần từ mọi IExceptionStatusMapping của assembly này.
builder.Services.AddSingleton(ExceptionStatusMap.FromAssembly(typeof(GlobalExceptionMiddleware).Assembly));
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddUrlSegmentVersioning();
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
{
    // FR-OBS-002 (Buổi 8): mọi Problem Details (lỗi nghiệp vụ, 400, 429, 500…) mang mã tra cứu cùng giá trị header X-Correlation-ID.
    var correlationId = CorrelationIdMiddleware.Get(ctx.HttpContext);
    if (correlationId is not null)
    {
        ctx.ProblemDetails.Extensions["correlationId"] = correlationId;
    }

    // Body vượt RequestSizeLimit (> 10MB) bị chặn trước khi tới handler → vẫn trả Application Error Code (Phụ lục B)
    if (ctx.ProblemDetails.Status == StatusCodes.Status413PayloadTooLarge)
    {
        ctx.ProblemDetails.Type = ErrorCodes.FileSizeExceeded;
        ctx.ProblemDetails.Detail = "Kích thước file vượt quá giới hạn 5MB.";
    }
});
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSecurityRateLimiting(builder.Configuration);
builder.Services.AddCulinaryOpenApi();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    // FR-SRCH-002: difficulty=Easy|Medium|Hard – enum dạng chuỗi
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// NFR-SEC-005: CORS chỉ cho phép origin cấu hình (không wildcard)
builder.Services.AddSecurityCors(builder.Configuration);

var app = builder.Build();

// API (migrate) và worker Hangfire (tạo schema) đều cần DB – chờ DB thay vì crash khi Docker khởi động song song.
// Host chỉ sinh tài liệu OpenAPI không chạm dữ liệu nên được phép bỏ qua bước chờ này.
if (!builder.Configuration.GetValue("Database:SkipWait", false))
{
    await app.Services.WaitForDatabaseAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(false);
}

// FR-JOB-003 (Buổi 4): lịch recurring job nằm trong storage Hangfire dùng chung — AddOrUpdate idempotent.
if (builder.Configuration.GetValue("Hangfire:RegisterRecurringJobs", true))
{
    app.Services.RegisterRecurringJobs();
}

if (workerOnly)
{
    app.MapGet("/", () => Results.Ok(new { service = "CulinaryBlog.Hangfire", mode = "worker" }));

    // Buổi 8 — healthcheck của container hangfire (8 service healthy): worker không phục vụ được job khi mất PostgreSQL/Redis.
    app.MapGet("/health/live", (HttpContext context, HealthCheckService healthChecks) => WriteLivenessResponseAsync(context, healthChecks));
    app.MapGet("/health/ready", (HttpContext context, HealthCheckService healthChecks) =>
        WriteHealthResponseAsync(context, healthChecks, check => check.Tags.Contains("ready")));
    await app.RunAsync().ConfigureAwait(false);
    return;
}

// Production chạy nhiều bản sao api (docker-compose.prod.yml): migrate + seed do MỘT container "migrate" chạy với
// Database:MigrateOnly=true rồi thoát; các bản sao api chỉ khởi động sau khi nó xong (service_completed_successfully), nên
// không có hai process cùng áp migration hay cùng chèn dữ liệu mẫu.
var migrateOnly = builder.Configuration.GetValue("Database:MigrateOnly", false);
if (migrateOnly || builder.Configuration.GetValue("Database:MigrateOnStartup", app.Environment.IsDevelopment()))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Database.MigrateAsync().ConfigureAwait(false);

    // D-6 (Buổi 4): Output Cache đã bị gỡ — cơ chế cache duy nhất là Redis cache-aside (NFR-PERF-003). Seeder tự xóa
    // "categories:all"; khóa công thức (recipes:list 2′, recipe:{slug} 5′) hết hạn tự nhiên theo TTL chuẩn.
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync(CancellationToken.None).ConfigureAwait(false);

    // NFR-PERF-004 (Buổi 5): bộ ≥ 10.000 công thức để đo EXPLAIN ANALYZE — chỉ chạy khi PerformanceSeed:Enabled=true (mặc định tắt).
    await scope.ServiceProvider.GetRequiredService<PerformanceSeeder>().SeedAsync(CancellationToken.None).ConfigureAwait(false);
}

if (migrateOnly)
{
    return;
}

app.UseRequestSecurity();

if (builder.Configuration.GetValue("OpenApi:Enabled", app.Environment.IsDevelopment()))
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.WithTitle("Culinary Blog API v1"));
}

// FR-JOB-001 / MT-39: Dashboard chỉ chạy ở container "api" (worker đã return ở trên).
// Hai lớp bảo vệ: Basic Auth tại Nginx + header X-Hangfire-Gate do Nginx gắn (NginxGateDashboardFilter).
if (builder.Configuration.GetValue("Hangfire:DashboardEnabled", true))
{
    app.MapHangfireDashboard(
        builder.Configuration.GetValue("Hangfire:DashboardPath", "/hangfire") ?? "/hangfire",
        new DashboardOptions
        {
            Authorization = [new NginxGateDashboardFilter(builder.Configuration["Hangfire:DashboardGateSecret"] ?? string.Empty)],
            DisplayStorageConnectionString = false,
            DashboardTitle = "Culinary Blog – Background Jobs",
            IsReadOnlyFunc = _ => false,
        });
}

app.MapGet("/", () => Results.Ok(new { service = "CulinaryBlog.API", version = "v1", docs = "/scalar" }))
    .ExcludeFromDescription();

// FR-OBS-001 / SRS §8.7 (Buổi 3): ba endpoint, ba mục đích — không phải ba bản sao.
//   /health       → mọi check (database, redis, minio) + JSON chi tiết; 503 khi Unhealthy (MinIO lỗi chỉ Degraded → 200).
//   /health/live  → không chạy check nào: process còn sống là 200 (lỗi thì Docker restart container).
//   /health/ready → chỉ check tag "ready" (PostgreSQL + Redis): 503 thì ngừng chuyển traffic, KHÔNG restart.
// Đi thẳng qua HealthCheckService, không qua MediatR: Docker gọi mỗi 10 giây nên phải phụ thuộc ít thành phần nhất.
app.MapGet("/health", (HttpContext context, HealthCheckService healthChecks) => WriteHealthResponseAsync(context, healthChecks, _ => true))
    .WithName("GetHealth")
    .WithSummary("FR-OBS-001 – Trạng thái toàn bộ dịch vụ phụ thuộc")
    .WithDescription("FR-OBS-001 — SRS §8.7. Kiểm tra PostgreSQL, Redis và MinIO; trả 503 khi hệ thống không khỏe mạnh.")
    .WithTags("Health")
    .WithMetadata(
        new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(object), ["application/json"]),
        new ProducesResponseTypeMetadata(StatusCodes.Status503ServiceUnavailable, typeof(ProblemDetails), ["application/problem+json"]))
    .AllowAnonymous();
app.MapGet("/health/live", (HttpContext context, HealthCheckService healthChecks) => WriteLivenessResponseAsync(context, healthChecks))
    .WithName("GetLiveness")
    .WithSummary("FR-OBS-001 – Kiểm tra tiến trình API còn sống")
    .WithDescription("FR-OBS-001 — SRS §8.7. Không chạy dependency check; trả text/plain để Docker kiểm tra liveness.")
    .WithTags("Health")
    .WithMetadata(new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(string), ["text/plain"]))
    .AllowAnonymous();
app.MapGet("/health/ready", (HttpContext context, HealthCheckService healthChecks) =>
        WriteHealthResponseAsync(context, healthChecks, check => check.Tags.Contains("ready")))
    .WithName("GetReadiness")
    .WithSummary("FR-OBS-001 – Kiểm tra API sẵn sàng nhận traffic")
    .WithDescription("FR-OBS-001 — SRS §8.7. Chỉ kiểm tra PostgreSQL và Redis theo tag ready.")
    .WithTags("Health")
    .WithMetadata(
        new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(object), ["application/json"]),
        new ProducesResponseTypeMetadata(StatusCodes.Status503ServiceUnavailable, typeof(ProblemDetails), ["application/problem+json"]))
    .AllowAnonymous();

var api = app.MapV1Api()
    .ProducesProblem(StatusCodes.Status429TooManyRequests);
api.MapAuthEndpoints();
api.MapUsersEndpoints();
api.MapCategoriesEndpoints();
api.MapRecipesGroup()
    .MapRecipesEndpoints()
    .MapRecipeQueryEndpoints()
    .MapRecipeLifecycleEndpoints();
api.MapFilesEndpoints();

await app.RunAsync().ConfigureAwait(false);

static async Task WriteHealthResponseAsync(
    HttpContext context,
    HealthCheckService healthChecks,
    Func<HealthCheckRegistration, bool> predicate)
{
    var report = await healthChecks.CheckHealthAsync(predicate, context.RequestAborted).ConfigureAwait(false);
    context.Response.StatusCode = report.Status == HealthStatus.Unhealthy
        ? StatusCodes.Status503ServiceUnavailable
        : StatusCodes.Status200OK;
    await UIResponseWriter.WriteHealthCheckUIResponse(context, report).ConfigureAwait(false);
}

static async Task WriteLivenessResponseAsync(HttpContext context, HealthCheckService healthChecks)
{
    var report = await healthChecks.CheckHealthAsync(_ => false, context.RequestAborted).ConfigureAwait(false);
    context.Response.ContentType = "text/plain; charset=utf-8";
    await context.Response.WriteAsync(report.Status.ToString(), context.RequestAborted).ConfigureAwait(false);
}

/// <summary>
/// Top-level statements sinh ra lớp Program ẩn. Khai báo tường minh để
/// <c>WebApplicationFactory&lt;Program&gt;</c> của integration test harness (NFR-MAINT-002) tham chiếu được.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// Constructor để non-public nhằm nói rõ Program chỉ là điểm vào của host, không phải kiểu để khởi tạo.
    /// </summary>
    protected Program()
    {
    }
}
