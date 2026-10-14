using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class RefreshTokenTests(CulinaryBlogApiFactory factory) : IClassFixture<CulinaryBlogApiFactory>
{
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task ConcurrentRefreshConsumesOnceAndReuseRevokesOnlyThatFamily(int requestCount)
    {
        using var setupClient = factory.CreateClient();
        var original = await RegisterAsync(setupClient).ConfigureAwait(true);
        var other = await RegisterAsync(setupClient).ConfigureAwait(true);
        using var login = await setupClient.PostAsJsonAsync("/api/v1/auth/login", new { original.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        login.EnsureSuccessStatusCode();
        var independent = (await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var arrivals = 0;
        using var concurrentFactory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IRefreshTokenRepository>(provider => new SynchronizedRefreshTokenRepository(
                new RefreshTokenRepository(provider.GetRequiredService<CulinaryBlogDbContext>()),
                async () =>
                {
                    if (Interlocked.Increment(ref arrivals) == requestCount)
                    {
                        barrier.TrySetResult();
                    }

                    await barrier.Task.WaitAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
                }))));
        using var client = concurrentFactory.CreateClient();
        var responses = await Task.WhenAll(Enumerable.Range(0, requestCount)
            .Select(_ => client.PostAsJsonAsync("/api/v1/auth/refresh", new { original.RefreshToken }))).ConfigureAwait(true);
        try
        {
            Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
            var losers = responses.Where(response => response.StatusCode == HttpStatusCode.Unauthorized).ToArray();
            Assert.Equal(requestCount - 1, losers.Length);
            foreach (var loser in losers)
            {
                using var problem = JsonDocument.Parse(await loser.Content.ReadAsStringAsync().ConfigureAwait(true));
                Assert.Equal("AUTH_REFRESH_TOKEN_REVOKED", problem.RootElement.GetProperty("type").GetString());
            }

            var winner = (await responses.Single(response => response.IsSuccessStatusCode).Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var userTokens = await db.RefreshTokens.Where(token => token.UserId == original.User.Id).ToListAsync().ConfigureAwait(true);
            Assert.Equal(3, userTokens.Count); // Original, second login session, exactly one replacement.
            Assert.All(userTokens.Where(token => token.TokenHash != tokens.HashToken(independent.RefreshToken)), token => Assert.NotNull(token.RevokedAt));
            Assert.Null(userTokens.Single(token => token.TokenHash == tokens.HashToken(independent.RefreshToken)).RevokedAt);
            Assert.Equal(tokens.HashToken(winner.RefreshToken), userTokens.Single(token => token.TokenHash == tokens.HashToken(original.RefreshToken)).ReplacedByTokenHash);
            Assert.Single(userTokens, token => token.TokenHash == tokens.HashToken(winner.RefreshToken));
            await RefreshAsync(setupClient, independent.RefreshToken).ConfigureAwait(true);
            await RefreshAsync(setupClient, other.RefreshToken).ConfigureAwait(true);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Fact]
    public async Task FailedReplacementInsertRollsBackConsumptionAndCanRetry()
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        var other = await RegisterAsync(client).ConfigureAwait(true);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
        var old = (await repository.GetByHashAsync(tokens.HashToken(original.RefreshToken), CancellationToken.None).ConfigureAwait(true))!;
        var now = DateTime.UtcNow;
        var duplicate = RefreshToken.Create(original.User.Id, tokens.HashToken(other.RefreshToken), now.AddDays(7), now, null);
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.TryRotateAsync(old, duplicate, now, CancellationToken.None)).ConfigureAwait(true);
        var persisted = await db.RefreshTokens.AsNoTracking().SingleAsync(token => token.Id == old.Id).ConfigureAwait(true);
        Assert.Null(persisted.RevokedAt);
        Assert.Null(persisted.ReplacedByTokenHash);
        Assert.False(await db.RefreshTokens.AnyAsync(token => token.Id == duplicate.Id).ConfigureAwait(true));
        var generated = tokens.CreateRefreshToken();
        var replacement = RefreshToken.Create(original.User.Id, generated.TokenHash, generated.ExpiresAt, now, null);
        Assert.True(await repository.TryRotateAsync(old, replacement, now, CancellationToken.None).ConfigureAwait(true));
        Assert.Equal(2, await db.RefreshTokens.CountAsync(token => token.UserId == original.User.Id).ConfigureAwait(true));
        await RefreshAsync(client, other.RefreshToken).ConfigureAwait(true);
    }

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReusingTokenRevokesDescendantsButPreservesIndependentSessions(bool reuseMiddle)
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        var other = await RegisterAsync(client).ConfigureAwait(true);
        using var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { original.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        loginResponse.EnsureSuccessStatusCode();
        var secondSession = (await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        var rotated = await RefreshAsync(client, original.RefreshToken).ConfigureAwait(true);
        var leaf = reuseMiddle ? await RefreshAsync(client, rotated.RefreshToken).ConfigureAwait(true) : rotated;
        await AssertErrorAsync(client, reuseMiddle ? rotated.RefreshToken : original.RefreshToken, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var userTokens = await db.RefreshTokens.Where(token => token.UserId == original.User.Id).ToListAsync().ConfigureAwait(true);
        Assert.All(userTokens.Where(token => token.TokenHash != tokens.HashToken(secondSession.RefreshToken)), token => Assert.NotNull(token.RevokedAt));
        Assert.Null(userTokens.Single(token => token.TokenHash == tokens.HashToken(secondSession.RefreshToken)).RevokedAt);
        await AssertErrorAsync(client, leaf.RefreshToken, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);
        await RefreshAsync(client, secondSession.RefreshToken).ConfigureAwait(true);
        await RefreshAsync(client, other.RefreshToken).ConfigureAwait(true);
    }

    [Fact]
    public async Task UnknownTokenReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        await AssertErrorAsync(client, "unknown-token", HttpStatusCode.Unauthorized, "AUTH_TOKEN_INVALID").ConfigureAwait(true);
    }

    [Theory]
    [InlineData("cycle")]
    [InlineData("missing")]
    [InlineData("other-user")]
    public async Task FamilyTraversalStopsSafelyAtMalformedLinks(string malformedLink)
    {
        using var client = factory.CreateClient();
        var independent = await RegisterAsync(client).ConfigureAwait(true);
        var other = await RegisterAsync(client).ConfigureAwait(true);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var now = DateTime.UtcNow;
        var rootHash = tokens.CreateRefreshToken().TokenHash;
        var childHash = tokens.CreateRefreshToken().TokenHash;
        var root = RefreshToken.Create(independent.User.Id, rootHash, now.AddDays(7), now, null);
        var child = RefreshToken.Create(independent.User.Id, childHash, now.AddDays(7), now, null);
        root.Revoke(now, childHash);
        db.RefreshTokens.AddRange(root, child);
        await db.SaveChangesAsync().ConfigureAwait(true);
        db.Entry(child).Property(token => token.ReplacedByTokenHash).CurrentValue = malformedLink switch
        {
            "cycle" => rootHash,
            "other-user" => tokens.HashToken(other.RefreshToken),
            _ => tokens.CreateRefreshToken().TokenHash,
        };
        await db.SaveChangesAsync().ConfigureAwait(true);
        await scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>()
            .RevokeFamilyAsync(independent.User.Id, rootHash, now, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(true);
        Assert.NotNull((await db.RefreshTokens.AsNoTracking().SingleAsync(token => token.Id == child.Id).ConfigureAwait(true)).RevokedAt);
        await RefreshAsync(client, independent.RefreshToken).ConfigureAwait(true);
        await RefreshAsync(client, other.RefreshToken).ConfigureAwait(true);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisableAndExplicitRevokeAllStillRevokeIndependentSessions(bool disableAccount)
    {
        using var client = factory.CreateClient();
        var original = await RegisterAsync(client).ConfigureAwait(true);
        var other = await RegisterAsync(client).ConfigureAwait(true);
        using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { original.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        login.EnsureSuccessStatusCode();
        await using var scope = factory.Services.CreateAsyncScope();
        if (disableAccount)
        {
            await scope.ServiceProvider.GetRequiredService<IIdentityService>().SetActiveAsync(original.User.Id, false, CancellationToken.None).ConfigureAwait(true);
        }
        else
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", original.AccessToken);
            using var response = await client.PostAsync("/api/v1/auth/sessions/revoke-all", null).ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.Equal(2, await db.RefreshTokens.CountAsync(token => token.UserId == original.User.Id).ConfigureAwait(true));
        Assert.False(await db.RefreshTokens.AnyAsync(token => token.UserId == original.User.Id && token.RevokedAt == null).ConfigureAwait(true));
        await RefreshAsync(client, other.RefreshToken).ConfigureAwait(true);
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

    private sealed class SynchronizedRefreshTokenRepository(IRefreshTokenRepository inner, Func<Task> synchronize) : IRefreshTokenRepository
    {
        public Task<T> ExecuteForUserAsync<T>(string userId, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken) => inner.ExecuteForUserAsync(userId, operation, cancellationToken);

        public Task AddAsync(RefreshToken token, CancellationToken cancellationToken) => inner.AddAsync(token, cancellationToken);

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            var token = await inner.GetByHashAsync(tokenHash, cancellationToken).ConfigureAwait(false);
            await synchronize().ConfigureAwait(false);
            return token;
        }

        public Task RevokeAsync(RefreshToken token, DateTime revokedAt, CancellationToken cancellationToken) => inner.RevokeAsync(token, revokedAt, cancellationToken);

        public Task<bool> TryRotateAsync(RefreshToken token, RefreshToken replacement, DateTime now, CancellationToken cancellationToken) => inner.TryRotateAsync(token, replacement, now, cancellationToken);

        public Task RevokeAllForUserAsync(string userId, DateTime revokedAt, CancellationToken cancellationToken) => inner.RevokeAllForUserAsync(userId, revokedAt, cancellationToken);

        public Task RevokeFamilyAsync(string userId, string startingTokenHash, DateTime revokedAt, CancellationToken cancellationToken) => inner.RevokeFamilyAsync(userId, startingTokenHash, revokedAt, cancellationToken);

        public Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(string userId, DateTime now, CancellationToken cancellationToken) => inner.GetActiveForUserAsync(userId, now, cancellationToken);

        public Task<RefreshToken?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken) => inner.GetByIdForUserAsync(id, userId, cancellationToken);
    }

    private sealed class RequestBodyDetectionFeature : IHttpRequestBodyDetectionFeature
    {
        public bool CanHaveBody => true;
    }
}
