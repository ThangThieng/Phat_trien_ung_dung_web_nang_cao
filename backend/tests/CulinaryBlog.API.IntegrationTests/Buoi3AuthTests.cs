using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions.Auth;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class Buoi3AuthTests(Buoi3AuthFactory factory) : IClassFixture<Buoi3AuthFactory>
{
    private const string JwtShape = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ0ZXN0In0.AQ";

    [Fact]
    public async Task GoogleCreatesAuthorThenLogsInLinkedAccount()
    {
        var email = $"google-{Guid.NewGuid():N}@example.com";
        using var host = WithGoogle(new VerifiedGoogleIdentity(Guid.NewGuid().ToString("N"), email, "Google User", null, true));
        using var client = host.CreateClient();

        using var first = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = JwtShape }).ConfigureAwait(true);
        Assert.Equal(200, (int)first.StatusCode);
        using var firstJson = await ParseAsync(first).ConfigureAwait(true);
        var firstUser = firstJson.RootElement.GetProperty("user");
        Assert.Equal(email, firstUser.GetProperty("email").GetString());
        Assert.Contains(firstUser.GetProperty("roles").EnumerateArray(), role => role.GetString() == "Author");
        var userId = firstUser.GetProperty("id").GetString();

        using var second = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = JwtShape }).ConfigureAwait(true);
        Assert.Equal(200, (int)second.StatusCode);
        using var secondJson = await ParseAsync(second).ConfigureAwait(true);
        Assert.Equal(userId, secondJson.RootElement.GetProperty("user").GetProperty("id").GetString());
    }

    [Fact]
    public async Task VerifiedExistingEmailLinksOnlyAfterVerification()
    {
        var email = $"linked-{Guid.NewGuid():N}@example.com";
        using var client = factory.CreateClient();
        using var registered = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Passw0rd!", displayName = "Local User" }).ConfigureAwait(true);
        Assert.Equal(201, (int)registered.StatusCode);
        using var originalJson = await ParseAsync(registered).ConfigureAwait(true);
        var userId = originalJson.RootElement.GetProperty("user").GetProperty("id").GetString();

        var subject = Guid.NewGuid().ToString("N");
        using var unverifiedHost = WithGoogle(new VerifiedGoogleIdentity(subject, email, "Google User", null, false));
        using var unverifiedClient = unverifiedHost.CreateClient();
        using var rejected = await unverifiedClient.PostAsJsonAsync("/api/v1/auth/google", new { idToken = JwtShape }).ConfigureAwait(true);
        await AssertProblemAsync(rejected, 400, ErrorCodes.AuthGoogleTokenInvalid).ConfigureAwait(true);

        using var verifiedHost = WithGoogle(new VerifiedGoogleIdentity(subject, email, "Google User", null, true));
        using var verifiedClient = verifiedHost.CreateClient();
        using var linked = await verifiedClient.PostAsJsonAsync("/api/v1/auth/google", new { idToken = JwtShape }).ConfigureAwait(true);
        Assert.Equal(200, (int)linked.StatusCode);
        using var linkedJson = await ParseAsync(linked).ConfigureAwait(true);
        Assert.Equal(userId, linkedJson.RootElement.GetProperty("user").GetProperty("id").GetString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("a.b.c")]
    public async Task MissingOrMalformedGoogleTokenReturns400(string token)
    {
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = token }).ConfigureAwait(true);
        Assert.Equal(400, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("invalid", 401, ErrorCodes.AuthGoogleTokenInvalid)]
    [InlineData("unavailable", 502, ErrorCodes.AuthGoogleUnavailable)]
    public async Task GoogleVerificationFailuresKeepTheirStatus(string scenario, int status, string code)
    {
        using var host = WithGoogle(null, scenario);
        using var client = host.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = JwtShape }).ConfigureAwait(true);
        await AssertProblemAsync(response, status, code).ConfigureAwait(true);
    }

    [Fact]
    public async Task DisabledGoogleAccountReturns403()
    {
        var email = $"disabled-{Guid.NewGuid():N}@example.com";
        using var client = factory.CreateClient();
        using var registered = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Passw0rd!", displayName = "Disabled" }).ConfigureAwait(true);
        Assert.Equal(201, (int)registered.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email).ConfigureAwait(true);
            Assert.NotNull(user);
            user.IsActive = false;
            Assert.True((await users.UpdateAsync(user).ConfigureAwait(true)).Succeeded);
        }

        using var host = WithGoogle(new VerifiedGoogleIdentity(Guid.NewGuid().ToString("N"), email, "Disabled", null, true));
        using var googleClient = host.CreateClient();
        using var response = await googleClient.PostAsJsonAsync("/api/v1/auth/google", new { idToken = JwtShape }).ConfigureAwait(true);
        await AssertProblemAsync(response, 403, ErrorCodes.AuthAccountDisabled).ConfigureAwait(true);
    }

    [Fact]
    public async Task LogoutTwiceRevokesTokenAndStillReturns204()
    {
        var email = $"logout-{Guid.NewGuid():N}@example.com";
        using var client = factory.CreateClient();
        using var registered = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Passw0rd!", displayName = "Logout" }).ConfigureAwait(true);
        Assert.Equal(201, (int)registered.StatusCode);
        using var json = await ParseAsync(registered).ConfigureAwait(true);
        var accessToken = json.RootElement.GetProperty("accessToken").GetString();
        var refreshToken = json.RootElement.GetProperty("refreshToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var first = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken }).ConfigureAwait(true);
        Assert.Equal(204, (int)first.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var saved = await repository.FindRefreshTokenByHashAsync(tokens.HashToken(refreshToken!), CancellationToken.None).ConfigureAwait(true);
            Assert.NotNull(saved?.RevokedAt);
        }

        using var second = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken }).ConfigureAwait(true);
        Assert.Equal(204, (int)second.StatusCode);
        using var unknown = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = "unknown" }).ConfigureAwait(true);
        Assert.Equal(204, (int)unknown.StatusCode);
    }

    [Fact]
    public async Task WrongPasswordLocksAccountWith423AndLockoutEnd()
    {
        var email = $"locked-{Guid.NewGuid():N}@example.com";
        using var client = factory.CreateClient();
        using var registered = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Passw0rd!", displayName = "Locked" }).ConfigureAwait(true);
        Assert.Equal(201, (int)registered.StatusCode);
        for (var attempt = 1; attempt < 5; attempt++)
        {
            using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Wrong1!" }).ConfigureAwait(true);
            await AssertProblemAsync(response, 401, ErrorCodes.AuthInvalidCredentials).ConfigureAwait(true);
        }

        using var locked = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Wrong1!" }).ConfigureAwait(true);
        await AssertProblemAsync(locked, 423, ErrorCodes.AuthAccountLocked).ConfigureAwait(true);
        using var problem = await ParseAsync(locked).ConfigureAwait(true);
        Assert.NotEqual(JsonValueKind.Null, problem.RootElement.GetProperty("lockoutEnd").ValueKind);
    }

    private WebApplicationFactory<Program> WithGoogle(VerifiedGoogleIdentity? identity, string? failure = null) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IGoogleIdTokenValidator>();
            services.AddSingleton<IGoogleIdTokenValidator>(new FakeGoogleValidator(identity, failure));
        }));

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));

    private static async Task AssertProblemAsync(HttpResponseMessage response, int status, string code)
    {
        Assert.Equal(status, (int)response.StatusCode);
        using var json = await ParseAsync(response).ConfigureAwait(true);
        Assert.Equal(code, json.RootElement.GetProperty("type").GetString());
    }

    private sealed class FakeGoogleValidator(VerifiedGoogleIdentity? identity, string? failure) : IGoogleIdTokenValidator
    {
        public Task<VerifiedGoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken) => failure switch
        {
            "invalid" => throw new InvalidTokenException("Invalid Google token", ErrorCodes.AuthGoogleTokenInvalid),
            "unavailable" => throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Google unavailable"),
            _ => Task.FromResult(identity!),
        };
    }
}
