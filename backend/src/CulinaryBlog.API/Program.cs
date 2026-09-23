using System.Text.Json.Serialization;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.API.Services;
using CulinaryBlog.Application;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.DataProtection;
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

// NFR-SEC-005: CORS chỉ cho phép origin cấu hình (không wildcard)
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
    .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID")));

var app = builder.Build();

// API (migrate) và worker Hangfire (tạo schema) đều cần DB – chờ DB thay vì crash khi Docker khởi động song song
await app.Services.WaitForDatabaseAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(false);

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

var api = app.MapGroup("/api/v1");
api.MapAuthEndpoints();
api.MapCategoriesEndpoints();
api.MapRecipesEndpoints();
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
