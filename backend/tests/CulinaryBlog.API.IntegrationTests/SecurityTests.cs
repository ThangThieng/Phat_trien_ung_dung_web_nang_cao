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

public sealed class SecurityTests(CulinaryBlogApiFactory factory) : IClassFixture<CulinaryBlogApiFactory>
{
    [Theory]
    [InlineData("http://localhost:3000", true)]
    [InlineData("https://untrusted.example", false)]
    public async Task CorsPreflightUsesWhitelistAndRequiredHeaders(string origin, bool allowed)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/refresh");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "PATCH");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type,x-correlation-id,if-match");
        using var response = await client.SendAsync(request).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
        if (allowed)
        {
            Assert.Equal(origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
            var methods = string.Join(',', response.Headers.GetValues("Access-Control-Allow-Methods"));
            foreach (var method in new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" })
            {
                Assert.Contains(method, methods, StringComparison.Ordinal);
            }

            Assert.Contains("If-Match", string.Join(',', response.Headers.GetValues("Access-Control-Allow-Headers")), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("http://localhost:3000", true)]
    [InlineData("https://untrusted.example", false)]
    public async Task CorsActualResponseUsesWhitelist(string origin, bool allowed)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Add("Origin", origin);
        using var response = await client.SendAsync(request).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
        if (allowed)
        {
            var exposed = string.Join(',', response.Headers.GetValues("Access-Control-Expose-Headers"));
            foreach (var header in new[] { "ETag", "Retry-After", "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset" })
            {
                Assert.Contains(header, exposed, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task ApiSecurityHeadersArePresentEvenOnUnauthorizedResponse()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/v1/auth/me").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        foreach (var directive in new[] { "script-src 'none'", "object-src 'none'", "frame-ancestors 'none'", "base-uri 'self'", "form-action 'self'" })
        {
            Assert.Contains(directive, csp, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task DisabledAccountCannotLogin()
    {
        using var client = factory.CreateClient();
        var email = $"disabled-{Guid.NewGuid():N}@example.com";
        using var registration = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "TestPassw0rd!", displayName = "Security Test" }).ConfigureAwait(true);
        registration.EnsureSuccessStatusCode();
        var auth = (await registration.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IIdentityService>().SetActiveAsync(auth.User.Id, false, CancellationToken.None).ConfigureAwait(true);
        using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Forbidden, login.StatusCode);
        using var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal("AUTH_ACCOUNT_DISABLED", body.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void ProtectedEndpointMetadataUsesCurrentPolicies()
    {
        using var client = factory.CreateClient();
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToArray();
        foreach (var endpoint in endpoints)
        {
            var route = NormalizeV1Route(endpoint.RoutePattern.RawText!);
            var policies = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            Assert.DoesNotContain(policies, policy => policy.Policy == "VerifiedAuthor");
            if (route.StartsWith("/api/v1/users", StringComparison.Ordinal))
            {
                Assert.Contains(policies, policy => policy.Policy == AuthorizationPolicies.Admin);
            }

            if (route.StartsWith("/api/v1/files", StringComparison.Ordinal))
            {
                Assert.Contains(policies, policy => policy.Policy == AuthorizationPolicies.Author);
            }

            if (route.StartsWith("/api/v1/auth/sessions", StringComparison.Ordinal) || route == "/api/v1/auth/me")
            {
                Assert.NotEmpty(policies);
            }
        }

        Assert.Contains(endpoints, endpoint => NormalizeV1Route(endpoint.RoutePattern.RawText!) == "/api/v1/users");
    }

    [Fact]
    public async Task OpenApiAndScalarAreAvailableWithVersionedAuthAnd429Metadata()
    {
        using var client = factory.CreateClient();
        using var document = await client.GetAsync("/openapi/v1.json").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, document.StatusCode);
        using var json = JsonDocument.Parse(await document.Content.ReadAsStringAsync().ConfigureAwait(true));
        foreach (var path in json.RootElement.GetProperty("paths").EnumerateObject())
        {
            Assert.StartsWith("/api/v1/", path.Name, StringComparison.Ordinal);
        }

        var refresh = json.RootElement.GetProperty("paths").GetProperty("/api/v1/auth/refresh").GetProperty("post");
        foreach (var status in new[] { "400", "401", "403", "429" })
        {
            Assert.True(refresh.GetProperty("responses").TryGetProperty(status, out _));
        }

        using var scalar = await client.GetAsync("/scalar").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, scalar.StatusCode);
        Assert.Contains("scalar", await scalar.Content.ReadAsStringAsync().ConfigureAwait(true), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeV1Route(string route) => route
        .Replace("{version:apiVersion}", "1", StringComparison.Ordinal)
        .Replace("{version}", "1", StringComparison.Ordinal)
        .TrimEnd('/');
}
