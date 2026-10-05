using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class RefreshTokenTests(CulinaryBlogApiFactory factory) : IClassFixture<CulinaryBlogApiFactory>
{
    [Fact]
    public async Task RotationPersistsOnlyHashAndRevokesOldToken()
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        var rotated = await RefreshAsync(client, original.RefreshToken).ConfigureAwait(true);
        Assert.NotEqual(original.RefreshToken, rotated.RefreshToken);
        Assert.Equal(32, Base64UrlEncoder.DecodeBytes(rotated.RefreshToken).Length);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var old = await db.RefreshTokens.SingleAsync(token => token.TokenHash == tokens.HashToken(original.RefreshToken)).ConfigureAwait(true);
        var replacement = await db.RefreshTokens.SingleAsync(token => token.TokenHash == tokens.HashToken(rotated.RefreshToken)).ConfigureAwait(true);
        Assert.NotNull(old.RevokedAt);
        Assert.Equal(replacement.TokenHash, old.ReplacedByTokenHash);
        Assert.Null(replacement.RevokedAt);
        Assert.NotEqual(rotated.RefreshToken, replacement.TokenHash);
    }

    [Fact]
    public async Task ReusingOldTokenRevokesAllSessionsButPreservesOtherUsers()
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        var other = await RegisterAsync(client).ConfigureAwait(true);
        using var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { original.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        loginResponse.EnsureSuccessStatusCode();
        var secondSession = (await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        var rotated = await RefreshAsync(client, original.RefreshToken).ConfigureAwait(true);
        await AssertErrorAsync(client, original.RefreshToken, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.False(await db.RefreshTokens.AnyAsync(token => token.UserId == original.User.Id && token.RevokedAt == null).ConfigureAwait(true));
        await AssertErrorAsync(client, rotated.RefreshToken, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);
        await AssertErrorAsync(client, secondSession.RefreshToken, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);
        await RefreshAsync(client, other.RefreshToken).ConfigureAwait(true);
    }

    [Fact]
    public async Task UnknownTokenReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        await AssertErrorAsync(client, "unknown-token", HttpStatusCode.Unauthorized, "AUTH_TOKEN_INVALID").ConfigureAwait(true);
    }

    [Fact]
    public async Task ExpiredTokenReturnsSpecificError()
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var generated = tokens.CreateRefreshToken();
        db.RefreshTokens.Add(RefreshToken.Create(original.User.Id, generated.TokenHash, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(-8), null));
        await db.SaveChangesAsync().ConfigureAwait(true);
        await AssertErrorAsync(client, generated.RawToken, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_EXPIRED").ConfigureAwait(true);
    }

    [Fact]
    public async Task DisabledAccountCannotRefresh()
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IIdentityService>().SetActiveAsync(original.User.Id, false, CancellationToken.None).ConfigureAwait(true);
        await AssertErrorAsync(client, original.RefreshToken, HttpStatusCode.Forbidden, "AUTH_ACCOUNT_DISABLED").ConfigureAwait(true);
    }

    [Theory]
    [InlineData("172.20.0.2", "203.0.113.42")]
    [InlineData("198.51.100.2", "198.51.100.2")]
    public async Task ForwardedIpIsAcceptedOnlyFromTrustedProxy(string proxyIp, string expectedIp)
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        var body = JsonSerializer.Serialize(new { refreshToken = original.RefreshToken });
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        var context = await factory.Server.SendAsync(http =>
        {
            http.Connection.RemoteIpAddress = IPAddress.Parse(proxyIp);
            http.Request.Method = "POST";
            http.Request.Scheme = "http";
            http.Request.Host = new Microsoft.AspNetCore.Http.HostString("localhost");
            http.Request.Path = "/api/v1/auth/refresh";
            http.Request.ContentType = "application/json";
            http.Request.ContentLength = stream.Length;
            http.Request.Body = stream;
            http.Features.Set<IHttpRequestBodyDetectionFeature>(new RequestBodyDetectionFeature());
            http.Request.Headers["X-Forwarded-For"] = "203.0.113.42";
        }).ConfigureAwait(true);
        using var responseReader = new StreamReader(context.Response.Body);
        var responseBody = await responseReader.ReadToEndAsync().ConfigureAwait(true);
        Assert.True(context.Response.StatusCode == 200, responseBody);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var replacement = await db.RefreshTokens.SingleAsync(token => token.UserId == original.User.Id && token.RevokedAt == null).ConfigureAwait(true);
        Assert.Equal(expectedIp, replacement.CreatedByIp);
    }

    private static async Task<AuthResponseDto> RegisterAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            displayName = "Refresh Test",
            email = $"refresh-{Guid.NewGuid():N}@example.com",
            password = "TestPassw0rd!",
        }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
    }

    private static async Task<AuthResponseDto> RefreshAsync(HttpClient client, string refreshToken)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
    }

    private static async Task AssertErrorAsync(HttpClient client, string refreshToken, HttpStatusCode status, string code)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken }).ConfigureAwait(true);
        Assert.Equal(status, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal(code, body.RootElement.GetProperty("type").GetString());
    }

    private sealed class RequestBodyDetectionFeature : IHttpRequestBodyDetectionFeature
    {
        public bool CanHaveBody => true;
    }
}
