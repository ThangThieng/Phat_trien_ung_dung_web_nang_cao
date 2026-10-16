using System.Net;
using System.Text;
using System.Text.Json;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.API.Middleware.ExceptionMapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.IntegrationTests;

/// <summary>Exercise the production security pipeline without PostgreSQL/Redis dependencies.</summary>
public sealed class SecurityMiddlewareTests
{
    private const string SigningKey = "security-pipeline-tests-only-key-at-least-32-bytes";

    [Theory]
    [InlineData("/api/v1/auth/register", 10)]
    [InlineData("/api/v1/auth/login", 10)]
    [InlineData("/api/v1/auth/google", 10)]
    [InlineData("/api/v1/auth/refresh", 10)]
    [InlineData("/api/v1/files/upload", 5)]
    [InlineData("/api/v1/users", 100)]
    public async Task ProductionQuotaAndRejectionContract(string path, int quota)
    {
        await using var app = await CreateAppAsync().ConfigureAwait(true);
        var server = app.GetTestServer();
        for (var index = 0; index < quota; index++)
        {
            var accepted = await SendAsync(server, path, "198.51.100.10").ConfigureAwait(true);
            Assert.NotEqual(429, accepted.Response.StatusCode);
            Assert.Equal((quota - index - 1).ToString(System.Globalization.CultureInfo.InvariantCulture), accepted.Response.Headers["X-RateLimit-Remaining"]);
        }

        var response = await SendAsync(server, path, "198.51.100.10").ConfigureAwait(true);
        Assert.Equal(429, response.Response.StatusCode);
        Assert.Equal(quota.ToString(System.Globalization.CultureInfo.InvariantCulture), response.Response.Headers["X-RateLimit-Limit"]);
        Assert.Equal("0", response.Response.Headers["X-RateLimit-Remaining"]);
        Assert.InRange(int.Parse(response.Response.Headers.RetryAfter.ToString(), System.Globalization.CultureInfo.InvariantCulture), 1, 60);
        Assert.InRange(
            long.Parse(response.Response.Headers["X-RateLimit-Reset"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            DateTimeOffset.UtcNow.AddSeconds(61).ToUnixTimeSeconds());
        using var body = await JsonDocument.ParseAsync(response.Response.Body).ConfigureAwait(true);
        Assert.Equal("RATE_LIMIT_EXCEEDED", body.RootElement.GetProperty("type").GetString());
        Assert.Equal(429, body.RootElement.GetProperty("status").GetInt32());
        Assert.StartsWith("application/problem+json", response.Response.ContentType, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("198.51.100.20", null, "198.51.100.21", null)]
    [InlineData("172.20.0.2", "203.0.113.20", "172.20.0.2", "203.0.113.21")]
    public async Task TrustedRealIpPartitionsRemainIndependent(string first, string? firstForwarded, string second, string? secondForwarded)
    {
        await using var app = await CreateAppAsync().ConfigureAwait(true);
        var server = app.GetTestServer();
        for (var index = 0; index < 10; index++)
        {
            var accepted = await SendAsync(server, "/api/v1/auth/me", first, firstForwarded).ConfigureAwait(true);
            Assert.Equal(401, accepted.Response.StatusCode);
        }

        Assert.Equal(429, (await SendAsync(server, "/api/v1/auth/me", first, firstForwarded).ConfigureAwait(true)).Response.StatusCode);
        Assert.Equal(401, (await SendAsync(server, "/api/v1/auth/me", second, secondForwarded).ConfigureAwait(true)).Response.StatusCode);
    }

    [Fact]
    public async Task UntrustedForwardedIpCannotEvadeQuota()
    {
        await using var app = await CreateAppAsync().ConfigureAwait(true);
        var server = app.GetTestServer();
        for (var index = 0; index < 10; index++)
        {
            await SendAsync(server, "/api/v1/auth/me", "198.51.100.30", $"203.0.113.{index + 1}").ConfigureAwait(true);
        }

        Assert.Equal(429, (await SendAsync(server, "/api/v1/auth/me", "198.51.100.30", "203.0.113.99").ConfigureAwait(true)).Response.StatusCode);
    }

    [Theory]
    [InlineData(null, 401)]
    [InlineData("Author", 403)]
    [InlineData("Admin", 204)]
    public async Task JwtAndAdminPolicyEnforceRoles(string? role, int status)
    {
        await using var app = await CreateAppAsync().ConfigureAwait(true);
        using var client = app.GetTestClient();
        if (role is not null)
        {
            var jwt = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = "CulinaryBlog",
                Audience = "CulinaryBlog.Client",
                Claims = new Dictionary<string, object> { ["sub"] = "pipeline-test", ["role"] = role },
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)), SecurityAlgorithms.HmacSha256),
            });
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        }

        using var response = await client.GetAsync("/api/v1/users").ConfigureAwait(true);
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Contains("frame-ancestors 'none'", Assert.Single(response.Headers.GetValues("Content-Security-Policy")), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("http://localhost:3000", true)]
    [InlineData("https://evil.example", false)]
    public async Task ProductionCorsHandlesPreflightWithoutConsumingAuthQuota(string origin, bool allowed)
    {
        await using var app = await CreateAppAsync().ConfigureAwait(true);
        using var client = app.GetTestClient();
        for (var index = 0; index < 11; index++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/refresh");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type,x-correlation-id,if-match");
            using var response = await client.SendAsync(request).ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
        }

        using var actual = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        actual.Headers.Add("Origin", origin);
        using var result = await client.SendAsync(actual).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        Assert.Equal(allowed, result.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("*")]
    [InlineData("https://*.example.com")]
    [InlineData("https://example.com/path")]
    [InlineData("not-an-origin")]
    [InlineData("https://user:password@example.com")]
    [InlineData("ftp://example.com")]
    public void UnsafeCorsConfigurationFailsClosed(string origin)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = origin,
        }).Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSecurityCors(configuration));
    }

    [Fact]
    public void CorsPolicyHasAnExplicitNameAndDoesNotAllowCredentials()
    {
        var services = new ServiceCollection();
        services.AddSecurityCors(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var policy = provider.GetRequiredService<IOptions<CorsOptions>>().Value.GetPolicy(SecurityExtensions.CorsPolicy);
        Assert.NotNull(policy);
        Assert.False(policy.SupportsCredentials);
    }

    [Theory]
    [InlineData("Auth", "PermitLimit")]
    [InlineData("Auth", "WindowSeconds")]
    [InlineData("General", "PermitLimit")]
    [InlineData("General", "WindowSeconds")]
    [InlineData("Upload", "PermitLimit")]
    [InlineData("Upload", "WindowSeconds")]
    public void InvalidQuotaConfigurationFailsValidation(string policy, string setting)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"RateLimiting:{policy}:{setting}"] = "0",
        }).Build();
        var services = new ServiceCollection();
        services.AddSecurityRateLimiting(configuration);
        using var provider = services.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<SecurityRateLimitingOptions>>().Value);
    }

    [Fact]
    public async Task VersioningPipelineRejectsV2AndReportsV1()
    {
        await using var app = await CreateAppAsync().ConfigureAwait(true);
        using var client = app.GetTestClient();
        using var v1 = await client.PostAsync("/api/v1/auth/refresh", null).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.NoContent, v1.StatusCode);
        Assert.Contains("1.0", string.Join(',', v1.Headers.GetValues("api-supported-versions")), StringComparison.Ordinal);
        using var v2 = await client.PostAsync("/api/v2/auth/refresh", null).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.BadRequest, v2.StatusCode);
        using var error = JsonDocument.Parse(await v2.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal("VALIDATION_ERROR", error.RootElement.GetProperty("type").GetString());
        using var openApi = await client.GetAsync("/openapi/v1.json").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
        using var document = JsonDocument.Parse(await openApi.Content.ReadAsStringAsync().ConfigureAwait(true));
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/auth/{action}", out _));
        Assert.DoesNotContain(paths.EnumerateObject(), path => path.Name.Contains("{version", StringComparison.Ordinal));
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = SigningKey,
            ["Jwt:Issuer"] = "CulinaryBlog",
            ["Jwt:Audience"] = "CulinaryBlog.Client",
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
        });
        builder.Services.AddUrlSegmentVersioning();
        builder.Services.AddProblemDetails();
        builder.Services.AddSingleton(ExceptionStatusMap.FromAssembly(typeof(GlobalExceptionMiddleware).Assembly));
        builder.Services.AddVersionedOpenApi();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddTrustedForwardedHeaders(builder.Configuration);
        builder.Services.AddSecurityCors(builder.Configuration);
        builder.Services.AddSecurityRateLimiting(builder.Configuration);
        var app = builder.Build();
        app.UseRequestSecurity();
        var api = app.MapV1Api();
        api.MapPost("/auth/{action}", () => Results.NoContent()).WithMetadata(new SecurityRateLimitMetadata(SecurityRateLimits.Auth));
        api.MapGet("/auth/me", () => Results.NoContent()).WithMetadata(new SecurityRateLimitMetadata(SecurityRateLimits.Auth)).RequireAuthorization();
        api.MapGet("/users", () => Results.NoContent()).RequireAuthorization(AuthorizationPolicies.Admin);
        api.MapPost("/files/upload", () => Results.NoContent())
            .WithMetadata(new SecurityRateLimitMetadata(SecurityRateLimits.Upload)).RequireAuthorization(AuthorizationPolicies.Author);
        app.MapOpenApi();
        await app.StartAsync().ConfigureAwait(true);
        return app;
    }

    private static Task<HttpContext> SendAsync(TestServer server, string path, string ip, string? forwarded = null) => server.SendAsync(context =>
    {
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        context.Request.Method = path is "/api/v1/users" or "/api/v1/auth/me" ? "GET" : "POST";
        context.Request.Path = path;
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost");
        if (forwarded is not null)
        {
            context.Request.Headers["X-Forwarded-For"] = forwarded;
        }
    });
}
