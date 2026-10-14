using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>Tạo tài liệu OpenAPI từ endpoint thật mà không khởi động các container phụ thuộc dữ liệu.</summary>
public sealed class OpenApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("OpenApi");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=openapi;Username=openapi;Password=openapi");
        builder.UseSetting("ConnectionStrings:Redis", "localhost:6379");
        builder.UseSetting("MinIO:Endpoint", "http://localhost:9000");
        builder.UseSetting("MinIO:PublicBaseUrl", "http://localhost:9000/culinary-blog");
        builder.UseSetting("MinIO:AccessKey", "openapi");
        builder.UseSetting("MinIO:SecretKey", "openapi");
        builder.UseSetting("MinIO:BucketName", "culinary-blog-openapi");
        builder.UseSetting("Jwt:SigningKey", "openapi-tests-only-signing-key-0123456789-0123456789-0123456789");
        builder.UseSetting("Database:SkipWait", "true");
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Seed:Enabled", "false");
        builder.UseSetting("Hangfire:ServerEnabled", "false");
        builder.UseSetting("Hangfire:WorkerOnly", "false");
        builder.UseSetting("Hangfire:RegisterRecurringJobs", "false");
        builder.UseSetting("Hangfire:DashboardGateSecret", "openapi-test-gate");
        builder.UseSetting("OpenApi:Enabled", "true");
        builder.ConfigureServices(services => services.RemoveAll<IHostedService>());
    }
}
