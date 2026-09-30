using System.Text.Json.Serialization;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.API.Middleware.ExceptionMapping;
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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Container "hangfire" dùng cùng image nhưng chỉ chạy BackgroundJobServer (SRS §6.5)
var workerOnly = builder.Configuration.GetValue<bool>("Hangfire:WorkerOnly");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Khóa DataProtection lưu bền trên volume dùng chung cho api + hangfire (không mất khi tạo lại container)
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("CulinaryBlog");
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } keysPath)
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.AddHttpContextAccessor();

// Buổi 3: bảng "domain exception → mã HTTP" dựng MỘT lần từ mọi IExceptionStatusMapping của assembly này.
builder.Services.AddSingleton(ExceptionStatusMap.FromAssembly(typeof(GlobalExceptionMiddleware).Assembly));

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
{
    // Body vượt RequestSizeLimit (> 10MB) bị chặn trước khi tới handler → vẫn trả Application Error Code (Phụ lục B)
    if (ctx.ProblemDetails.Status == StatusCodes.Status413PayloadTooLarge)
    {
        ctx.ProblemDetails.Type = ErrorCodes.FileSizeExceeded;
        ctx.ProblemDetails.Detail = "Kích thước file vượt quá giới hạn 5MB.";
    }
});
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCulinaryOutputCache(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    // FR-SRCH-002: difficulty=Easy|Medium|Hard – enum dạng chuỗi
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Buổi 4 (Dev 1, MT-38): IP thật của client. Nginx gắn X-Forwarded-For; API chỉ tin header này khi request đến từ dải
// mạng Docker nội bộ (cấu hình được). Không bật thì CreatedByIp của refresh token ghi IP container Nginx — dữ liệu audit
// sai vĩnh viễn, và danh sách phiên (FR-AUTH-009) hiển thị cùng một IP cho mọi thiết bị.
// Dùng KnownIPNetworks + System.Net.IPNetwork: KnownNetworks + HttpOverrides.IPNetwork đã lỗi thời trên .NET 10
// (TreatWarningsAsErrors → gãy build).
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
    foreach (var cidr in builder.Configuration.GetSection("ForwardedHeaders:TrustedNetworks").Get<string[]>() ?? ["172.16.0.0/12"])
    {
        o.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(cidr));
    }
});

// NFR-SEC-005: CORS chỉ cho phép origin cấu hình (không wildcard)
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
    .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID")));

var app = builder.Build();

// API (migrate) và worker Hangfire (tạo schema) đều cần DB – chờ DB thay vì crash khi Docker khởi động song song
await app.Services.WaitForDatabaseAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(false);

// FR-JOB-003 (Buổi 4): lịch recurring job nằm trong storage Hangfire dùng chung — AddOrUpdate idempotent.
if (builder.Configuration.GetValue("Hangfire:RegisterRecurringJobs", true))
{
    app.Services.RegisterRecurringJobs();
}

if (workerOnly)
{
    app.MapGet("/", () => Results.Ok(new { service = "CulinaryBlog.Hangfire", mode = "worker" }));
    await app.RunAsync().ConfigureAwait(false);
    return;
}

if (builder.Configuration.GetValue("Database:MigrateOnStartup", app.Environment.IsDevelopment()))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    if (await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync(CancellationToken.None).ConfigureAwait(false))
    {
        // Dữ liệu mẫu vừa được bổ sung/sửa → bỏ response công thức đang nằm trong Output Cache (tag "recipes")
        try
        {
            await scope.ServiceProvider.GetRequiredService<IOutputCacheStore>()
                .EvictByTagAsync(OutputCachePolicies.RecipesTag, CancellationToken.None).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            // NFR-REL-002: Redis lỗi không được chặn API khởi động; cache cũ tự hết hạn theo TTL.
        }
    }
}

// ĐẦU pipeline: mọi middleware phía sau (Authentication, Rate Limiter ở Buổi 6) đều thấy IP thật.
app.UseForwardedHeaders();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.WithTitle("Culinary Blog API v1"));
}

// FR-JOB-001 / MT-39: Dashboard chỉ chạy ở container "api" (worker đã return ở trên).
// Hai lớp bảo vệ: Basic Auth tại Nginx + header X-Hangfire-Gate do Nginx gắn (NginxGateDashboardFilter).
app.MapHangfireDashboard(
    builder.Configuration.GetValue("Hangfire:DashboardPath", "/hangfire") ?? "/hangfire",
    new DashboardOptions
    {
        Authorization = [new NginxGateDashboardFilter(builder.Configuration["Hangfire:DashboardGateSecret"] ?? string.Empty)],
        DisplayStorageConnectionString = false,
        DashboardTitle = "Culinary Blog – Background Jobs",
        IsReadOnlyFunc = _ => false,
    });

app.MapGet("/", () => Results.Ok(new { service = "CulinaryBlog.API", version = "v1", docs = "/scalar" }))
    .ExcludeFromDescription();

// FR-OBS-001 / SRS §8.7 (Buổi 3): ba endpoint, ba mục đích — không phải ba bản sao.
//   /health       → mọi check (database, redis, minio) + JSON chi tiết; 503 khi Unhealthy (MinIO lỗi chỉ Degraded → 200).
//   /health/live  → không chạy check nào: process còn sống là 200 (lỗi thì Docker restart container).
//   /health/ready → chỉ check tag "ready" (PostgreSQL + Redis): 503 thì ngừng chuyển traffic, KHÔNG restart.
// Đi thẳng qua HealthCheckService, không qua MediatR: Docker gọi mỗi 10 giây nên phải phụ thuộc ít thành phần nhất.
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse })
    .AllowAnonymous();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
    .AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    })
    .AllowAnonymous();

var api = app.MapGroup("/api/v1");
api.MapAuthEndpoints();
api.MapUsersEndpoints();
api.MapCategoriesEndpoints();
api.MapRecipesGroup()
    .MapRecipesEndpoints()
    .MapRecipeLifecycleEndpoints();
api.MapFilesEndpoints();

await app.RunAsync().ConfigureAwait(false);

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
