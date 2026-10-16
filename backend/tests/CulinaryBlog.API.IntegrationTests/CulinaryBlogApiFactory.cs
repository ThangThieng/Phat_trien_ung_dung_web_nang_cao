using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class CulinaryBlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync()).ConfigureAwait(false);
        using var client = CreateClient();
        await using var scope = Services.CreateAsyncScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var result = await roles.CreateAsync(new IdentityRole("Author")).ConfigureAwait(false);
        Assert.True(result.Succeeded);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync().ConfigureAwait(false);
        await _redis.DisposeAsync().ConfigureAwait(false);
        await _postgres.DisposeAsync().ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
            ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
            ["Jwt:SigningKey"] = "integration-tests-only-signing-key-at-least-32-bytes",
            ["MinIO:Endpoint"] = "http://localhost:9000",
            ["MinIO:PublicBaseUrl"] = "http://localhost:9000/culinary-blog",
            ["MinIO:AccessKey"] = "integration-test",
            ["MinIO:SecretKey"] = "integration-test",
            ["Hangfire:ServerEnabled"] = "false",
            ["Hangfire:WorkerOnly"] = "false",
            ["Database:MigrateOnStartup"] = "true",
            ["Seed:Enabled"] = "false",
            ["DataProtection:KeysPath"] = null,
        };

        foreach (var (key, value) in settings)
        {
            builder.UseSetting(key, value);
        }

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
        builder.ConfigureTestServices(services =>
        {
            var initializer = services.Single(descriptor => descriptor.ServiceType == typeof(IHostedService) &&
                descriptor.ImplementationType == typeof(MinioBucketInitializer));
            services.Remove(initializer);
            services.RemoveAll<IWelcomeEmailScheduler>();
            services.AddSingleton<IWelcomeEmailScheduler, NoOpWelcomeEmailScheduler>();
        });
    }

    private sealed class NoOpWelcomeEmailScheduler : IWelcomeEmailScheduler
    {
        public void ScheduleWelcomeEmail(string userId)
        {
        }
    }
}
