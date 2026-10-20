using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Auth;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests;

[Collection(IntegrationTestSuite.Name)]
public sealed class AuthRequestContractTests(CulinaryBlogApiFactory factory)
{
    private const string ValidJwtShape = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ0ZXN0In0.AQ";

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"refreshToken\":null}")]
    [InlineData("{\"refreshToken\":\"\"}")]
    [InlineData("{\"refreshToken\":\" \\t\\r\\n \"}")]
    public async Task InvalidRefreshRequestReturnsBadRequestWithoutTokenLookup(string? body)
    {
        var tokens = new LookupProbe();
        using var isolated = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<IUserRepository>(tokens)));
        using var client = isolated.CreateClient();
        using var response = await PostBodyAsync(client, "/api/v1/auth/refresh", body).ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_ERROR").ConfigureAwait(true);
        Assert.Equal(0, tokens.Lookups);
    }

    [Fact]
    public async Task UnknownNonEmptyRefreshTokenStillReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = "unknown-non-empty-token" }).ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.Unauthorized, "AUTH_TOKEN_INVALID").ConfigureAwait(true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"idToken\":null}")]
    [InlineData("{\"idToken\":\"\"}")]
    [InlineData("{\"idToken\":\" \\t\\r\\n \"}")]
    [InlineData("{\"idToken\":\"not-a-jwt\"}")]
    [InlineData("{\"idToken\":\"a.b.c\"}")]
    [InlineData("{\"idToken\":\"eyJhbGciOiJSUzI1NiJ9..AQ\"}")]
    [InlineData("{\"idToken\":\"e30.W10.AQ\"}")]
    public async Task InvalidGoogleRequestUsesAuthCodeWithoutCallingVerification(string? body)
    {
        var validator = new GoogleValidationProbe("must-not-call");
        using var isolated = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<IGoogleIdTokenValidator>(validator)));
        using var client = isolated.CreateClient();
        using var response = await PostBodyAsync(client, "/api/v1/auth/google", body).ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "AUTH_GOOGLE_TOKEN_INVALID").ConfigureAwait(true);
        Assert.Equal(0, validator.Verifications);
    }

    [Theory]
    [InlineData("signature", HttpStatusCode.Unauthorized, "AUTH_GOOGLE_TOKEN_INVALID")]
    [InlineData("expired", HttpStatusCode.Unauthorized, "AUTH_GOOGLE_TOKEN_INVALID")]
    [InlineData("claims", HttpStatusCode.BadRequest, "AUTH_GOOGLE_TOKEN_INVALID")]
    [InlineData("unavailable", HttpStatusCode.BadGateway, "AUTH_GOOGLE_UNAVAILABLE")]
    [InlineData("disabled", HttpStatusCode.Forbidden, "AUTH_ACCOUNT_DISABLED")]
    public async Task GoogleVerificationOutcomesPreserveTheirStatusAndCode(string scenario, HttpStatusCode status, string code)
    {
        var validator = new GoogleValidationProbe(scenario);
        var identity = new GoogleIdentityProbe();
        using var isolated = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IGoogleIdTokenValidator>(validator);
            services.AddSingleton<IIdentityService>(identity);
        }));
        using var client = isolated.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = ValidJwtShape }).ConfigureAwait(true);
        await AssertProblemAsync(response, status, code).ConfigureAwait(true);
        Assert.Equal(1, validator.Verifications);
    }

    private static async Task<HttpResponseMessage> PostBodyAsync(HttpClient client, string path, string? body)
    {
        using var content = body is null ? null : new StringContent(body, Encoding.UTF8, "application/json");
        return await client.PostAsync(path, content).ConfigureAwait(true);
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal(expectedCode, json.RootElement.GetProperty("type").GetString());
        Assert.Equal((int)expectedStatus, json.RootElement.GetProperty("status").GetInt32());
    }

    private sealed class LookupProbe : IUserRepository
    {
        private int _lookups;

        public int Lookups => _lookups;

        public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _lookups);
            throw new InvalidOperationException("Invalid request reached token lookup.");
        }

        public Task<T> ExecuteForUserAsync<T>(string userId, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<RefreshToken?> GetRefreshTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> RevokeRefreshTokenIfActiveAsync(Guid tokenId, DateTime utcNow, string? replacedByTokenHash, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<int> RevokeAllRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task RevokeRefreshTokenFamilyAsync(string userId, string startingTokenHash, DateTime utcNow, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<RefreshToken>> GetActiveRefreshTokensAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<RefreshToken?> GetRefreshTokenByIdAsync(string userId, Guid tokenId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<string>> GetUserNamesStartingWithAsync(string prefix, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<PagedResult<UserAdminDto>> GetUsersPageAsync(UserListCriteria criteria, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class GoogleValidationProbe(string scenario) : IGoogleIdTokenValidator
    {
        private int _verifications;

        public int Verifications => _verifications;

        public Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _verifications);
            return scenario switch
            {
                "signature" or "expired" => throw InvalidTokenException.GoogleTokenRejected(),
                "claims" => throw new BusinessRuleViolationException(ErrorCodes.AuthGoogleTokenInvalid, "Required verified claims are missing."),
                "unavailable" => throw new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Verification unavailable."),
                "disabled" => Task.FromResult(new GoogleIdTokenPayload("disabled-sub", "disabled@example.com", true, "Disabled", null)),
                _ => throw new InvalidOperationException("Invalid request reached Google verification."),
            };
        }
    }

    private sealed class GoogleIdentityProbe : IIdentityService
    {
        public Task<IdentityUserInfo> FindOrCreateGoogleUserAsync(GoogleIdTokenPayload payload, CancellationToken cancellationToken) =>
            Task.FromResult(new IdentityUserInfo("disabled-user", "Disabled", "disabled@example.com", "disabled", null, false, []));

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityUserInfo?> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> IsLockedOutAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityUserInfo?> UpdateProfileAsync(string userId, ProfileChanges changes, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityUserInfo?> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityUserInfo> CreateUserAsync(string displayName, string email, string password, string role, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
