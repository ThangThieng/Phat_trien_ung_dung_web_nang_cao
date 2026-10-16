using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class RateLimitingTests(Dev1CulinaryBlogApiFactory factory) : IClassFixture<Dev1CulinaryBlogApiFactory>
{
    [Theory]
    [InlineData("/api/v1/auth/register", 10)]
    [InlineData("/api/v1/auth/login", 10)]
    [InlineData("/api/v1/auth/google", 10)]
    [InlineData("/api/v1/auth/refresh", 10)]
    [InlineData("/api/v1/files/upload", 5)]
    [InlineData("/api/v1/users", 100)]
    public async Task QuotaRejectsOnlyRequestAfterLimit(string path, int limit)
    {
        using var isolated = CreateRateLimitedHost();
        using var client = isolated.CreateClient();
        for (var index = 0; index < limit; index++)
        {
            var accepted = await SendFromAsync(isolated.Server, path, "198.51.100.10").ConfigureAwait(true);
            Assert.NotEqual(StatusCodes.Status429TooManyRequests, accepted.Response.StatusCode);
            Assert.Equal(limit.ToString(System.Globalization.CultureInfo.InvariantCulture), accepted.Response.Headers["X-RateLimit-Limit"]);
            Assert.Equal((limit - index - 1).ToString(System.Globalization.CultureInfo.InvariantCulture), accepted.Response.Headers["X-RateLimit-Remaining"]);
        }

        var rejected = await SendFromAsync(isolated.Server, path, "198.51.100.10").ConfigureAwait(true);
        Assert.Equal(StatusCodes.Status429TooManyRequests, rejected.Response.StatusCode);
        Assert.Equal("0", rejected.Response.Headers["X-RateLimit-Remaining"]);
        var retry = int.Parse(rejected.Response.Headers.RetryAfter.ToString(), System.Globalization.CultureInfo.InvariantCulture);
        Assert.InRange(retry, 1, 60);
        var reset = long.Parse(rejected.Response.Headers["X-RateLimit-Reset"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
        Assert.InRange(reset, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), DateTimeOffset.UtcNow.AddSeconds(61).ToUnixTimeSeconds());
        Assert.StartsWith("application/problem+json", rejected.Response.ContentType, StringComparison.Ordinal);
        using var body = await JsonDocument.ParseAsync(rejected.Response.Body).ConfigureAwait(true);
        Assert.Equal("RATE_LIMIT_EXCEEDED", body.RootElement.GetProperty("type").GetString());
        Assert.Equal(429, body.RootElement.GetProperty("status").GetInt32());
    }

    [Theory]
    [InlineData("198.51.100.20", null, "198.51.100.21", null)]
    [InlineData("172.20.0.2", "203.0.113.20", "172.20.0.2", "203.0.113.21")]
    public async Task DistinctRealIpsDoNotShareQuota(string first, string? forwardedFirst, string second, string? forwardedSecond)
    {
        using var isolated = CreateRateLimitedHost();
        using var client = isolated.CreateClient();
        for (var index = 0; index < 10; index++)
        {
            var response = await SendFromAsync(isolated.Server, "/api/v1/auth/me", first, forwardedFirst).ConfigureAwait(true);
            Assert.Equal(StatusCodes.Status401Unauthorized, response.Response.StatusCode);
        }

        var exhausted = await SendFromAsync(isolated.Server, "/api/v1/auth/me", first, forwardedFirst).ConfigureAwait(true);
        Assert.Equal(StatusCodes.Status429TooManyRequests, exhausted.Response.StatusCode);
        var independent = await SendFromAsync(isolated.Server, "/api/v1/auth/me", second, forwardedSecond).ConfigureAwait(true);
        Assert.Equal(StatusCodes.Status401Unauthorized, independent.Response.StatusCode);
    }

    [Fact]
    public async Task UntrustedForwardedHeaderCannotBypassQuota()
    {
        using var isolated = CreateRateLimitedHost();
        using var client = isolated.CreateClient();
        for (var index = 0; index < 10; index++)
        {
            await SendFromAsync(isolated.Server, "/api/v1/auth/me", "198.51.100.30", $"203.0.113.{index + 1}").ConfigureAwait(true);
        }

        var response = await SendFromAsync(isolated.Server, "/api/v1/auth/me", "198.51.100.30", "203.0.113.99").ConfigureAwait(true);
        Assert.Equal(StatusCodes.Status429TooManyRequests, response.Response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/auth/me", 3)]
    [InlineData("/api/v1/files/upload", 2)]
    [InlineData("/api/v1/users", 6)]
    public async Task ConfiguredSmallQuotasAreDeterministic(string path, int quota)
    {
        using var isolated = CreateRateLimitedHost(options =>
        {
            options.Auth = new RateQuota { PermitLimit = 3, WindowSeconds = 3600 };
            options.Upload = new RateQuota { PermitLimit = 2, WindowSeconds = 3600 };
            options.General = new RateQuota { PermitLimit = 6, WindowSeconds = 3600 };
        });
        using var client = isolated.CreateClient();
        for (var index = 0; index < quota; index++)
        {
            Assert.NotEqual(429, (await SendFromAsync(isolated.Server, path, "198.51.100.50").ConfigureAwait(true)).Response.StatusCode);
        }

        Assert.Equal(429, (await SendFromAsync(isolated.Server, path, "198.51.100.50").ConfigureAwait(true)).Response.StatusCode);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/healthz")]
    [InlineData("/hangfire")]
    [InlineData("/hangfire/jobs")]
    public async Task OperationalPathsDoNotConsumeGlobalQuota(string path)
    {
        using var isolated = CreateRateLimitedHost(options => options.General = new RateQuota { PermitLimit = 2, WindowSeconds = 3600 });
        using var client = isolated.CreateClient();
        for (var index = 0; index < 5; index++)
        {
            Assert.NotEqual(429, (await SendFromAsync(isolated.Server, path, "198.51.100.60").ConfigureAwait(true)).Response.StatusCode);
        }

        Assert.NotEqual(429, (await SendFromAsync(isolated.Server, "/api/v1/users", "198.51.100.60").ConfigureAwait(true)).Response.StatusCode);
        Assert.NotEqual(429, (await SendFromAsync(isolated.Server, "/api/v1/users", "198.51.100.60").ConfigureAwait(true)).Response.StatusCode);
        Assert.Equal(429, (await SendFromAsync(isolated.Server, "/api/v1/users", "198.51.100.60").ConfigureAwait(true)).Response.StatusCode);
    }

    [Fact]
    public async Task GlobalBucketIsSharedAcrossAuthAndGeneralRoutes()
    {
        using var isolated = CreateRateLimitedHost(options => options.General = new RateQuota { PermitLimit = 2, WindowSeconds = 3600 });
        using var client = isolated.CreateClient();
        Assert.NotEqual(429, (await SendFromAsync(isolated.Server, "/api/v1/auth/login", "198.51.100.70").ConfigureAwait(true)).Response.StatusCode);
        Assert.NotEqual(429, (await SendFromAsync(isolated.Server, "/api/v1/users", "198.51.100.70").ConfigureAwait(true)).Response.StatusCode);
        Assert.Equal(429, (await SendFromAsync(isolated.Server, "/api/v1/auth/me", "198.51.100.70").ConfigureAwait(true)).Response.StatusCode);
    }

    [Fact]
    public async Task DefaultIntegrationHostDoesNotEnforceLimiter()
    {
        using var client = factory.CreateClient();
        for (var index = 0; index < 12; index++)
        {
            using var response = await client.GetAsync("/api/v1/auth/me").ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateRateLimitedHost(Action<SecurityRateLimitingOptions>? configure = null) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.PostConfigure<SecurityRateLimitingOptions>(options =>
            {
                options.Enabled = true;
                configure?.Invoke(options);
            })));

    private static Task<HttpContext> SendFromAsync(TestServer server, string path, string ip, string? forwarded = null) =>
        server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
            context.Request.Method = path is "/api/v1/users" or "/api/v1/auth/me" || path.StartsWith("/health", StringComparison.Ordinal) || path.StartsWith("/hangfire", StringComparison.Ordinal) ? "GET" : "POST";
            context.Request.Path = path;
            context.Request.Host = new HostString("localhost");
            context.Request.Scheme = "http";
            if (forwarded is not null)
            {
                context.Request.Headers["X-Forwarded-For"] = forwarded;
            }
        });
}
