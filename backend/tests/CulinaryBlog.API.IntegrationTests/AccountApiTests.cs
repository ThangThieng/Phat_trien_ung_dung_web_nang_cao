using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class AccountApiTests(CulinaryBlogApiFactory factory) : IClassFixture<CulinaryBlogApiFactory>
{
    private static readonly string[] ProfileFields = ["avatarUrl", "bio", "createdAt", "displayName", "email", "id", "roles"];
    private static readonly string[] AdminFields = ["avatarUrl", "createdAt", "displayName", "email", "id", "isActive", "recipeCount", "roles"];
    private static readonly string[] SessionFields = ["createdAt", "createdByIp", "expiresAt", "id", "isCurrent"];
    private static readonly string[] AuthorRoles = ["Author"];
    private static readonly string[] AdminRoles = ["Admin"];

    [Theory]
    [InlineData("GET", "/api/v1/auth/me")]
    [InlineData("PATCH", "/api/v1/auth/me")]
    [InlineData("GET", "/api/v1/users")]
    [InlineData("PATCH", "/api/v1/users/missing/status")]
    [InlineData("GET", "/api/v1/auth/sessions")]
    [InlineData("DELETE", "/api/v1/auth/sessions/00000000-0000-0000-0000-000000000001")]
    [InlineData("POST", "/api/v1/auth/sessions/revoke-all")]
    public async Task AccountEndpointsRequireAuthentication(string method, string path)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        using var response = await client.SendAsync(request).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProfilePatchPreservesOmittedFieldsAndCannotChangeIdentityOrRoles()
    {
        using var client = factory.CreateClient();
        var user = await RegisterAsync(client).ConfigureAwait(true);
        using var initial = await client.GetAsync("/api/v1/auth/me").ConfigureAwait(true);
        Assert.True(initial.Headers.CacheControl?.NoStore);
        using var json = JsonDocument.Parse(await initial.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal(ProfileFields, json.RootElement.EnumerateObject().Select(property => property.Name).Order().ToArray());

        using var patch = await client.PatchAsJsonAsync("/api/v1/auth/me", new
        {
            displayName = "Updated Name",
            bio = "My kitchen",
            avatarUrl = "http://localhost:9000/culinary-blog/uploads/avatar.png",
            email = "attacker@example.com",
            userName = "attacker",
            roles = AdminRoles,
        }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
        var updated = (await patch.Content.ReadFromJsonAsync<UserProfileDto>().ConfigureAwait(true))!;
        Assert.Equal(user.User.Email, updated.Email);
        Assert.Equal("Updated Name", updated.DisplayName);
        Assert.Equal(AuthorRoles, updated.Roles);

        using var clear = await client.PatchAsJsonAsync("/api/v1/auth/me", new { bio = (string?)null, avatarUrl = (string?)null }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, clear.StatusCode);
        var cleared = (await clear.Content.ReadFromJsonAsync<UserProfileDto>().ConfigureAwait(true))!;
        Assert.Null(cleared.Bio);
        Assert.Null(cleared.AvatarUrl);
        Assert.Equal(updated.DisplayName, cleared.DisplayName);
        var persisted = await client.GetFromJsonAsync<UserProfileDto>("/api/v1/auth/me").ConfigureAwait(true);
        Assert.Equal(cleared.DisplayName, persisted!.DisplayName);
        Assert.Null(persisted.Bio);
    }

    [Theory]
    [InlineData("https://outside.example/avatar.png")]
    [InlineData("http://localhost:9000/culinary-blog/../other/avatar.png")]
    [InlineData("http://localhost:9000/culinary-blog/%2e%2e/other/avatar.png")]
    [InlineData("http://localhost:9000/CULINARY-BLOG/avatar.png")]
    [InlineData("http://localhost:9000/culinary-blog-other/avatar.png")]
    [InlineData("http://localhost:9000@outside.example/culinary-blog/avatar.png")]
    public async Task AvatarMustStayInsideConfiguredBucket(string avatarUrl)
    {
        using var client = factory.CreateClient();
        await RegisterAsync(client).ConfigureAwait(true);
        using var response = await client.PatchAsJsonAsync("/api/v1/auth/me", new { avatarUrl }).ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_ERROR").ConfigureAwait(true);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("avatarUrl", out _));
    }

    [Theory]
    [InlineData("{\"displayName\":null}", "displayName")]
    [InlineData("{\"displayName\":\"x\"}", "displayName")]
    [InlineData("{\"bio\":123}", null)]
    public async Task InvalidProfilePatchReturnsBadRequest(string body, string? field)
    {
        using var client = factory.CreateClient();
        await RegisterAsync(client).ConfigureAwait(true);
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PatchAsync("/api/v1/auth/me", content).ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_ERROR").ConfigureAwait(true);
        if (field is not null)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
            Assert.True(json.RootElement.GetProperty("errors").TryGetProperty(field, out _));
        }
    }

    [Fact]
    public async Task MissingUserWithOtherwiseValidJwtReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var user = await RegisterAsync(client).ConfigureAwait(true);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        await db.Users.Where(item => item.Id == user.User.Id).ExecuteDeleteAsync().ConfigureAwait(true);
        using var response = await client.GetAsync("/api/v1/auth/me").ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.Unauthorized, "AUTH_TOKEN_INVALID").ConfigureAwait(true);
    }

    [Fact]
    public async Task AuthorCannotListUsersOrChangeAccountStatus()
    {
        using var client = factory.CreateClient();
        var user = await RegisterAsync(client).ConfigureAwait(true);
        using var list = await client.GetAsync("/api/v1/users").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        using var patch = await client.PatchAsJsonAsync($"/api/v1/users/{user.User.Id}/status", new { isActive = false }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Forbidden, patch.StatusCode);
    }

    [Fact]
    public async Task AdminCanSearchDisableAndEnableUserAndRevokesAllTheirSessions()
    {
        using var client = factory.CreateClient();
        var target = await RegisterAsync(client).ConfigureAwait(true);
        using var secondLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new { target.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        secondLogin.EnsureSuccessStatusCode();
        var second = (await secondLogin.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        var admin = await RegisterAsync(client, admin: true).ConfigureAwait(true);

        using var list = await client.GetAsync($"/api/v1/users?search={Uri.EscapeDataString(target.User.Email)}&isActive=true&pageSize=1").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.True(list.Headers.CacheControl?.NoStore);
        using var json = JsonDocument.Parse(await list.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal(1, json.RootElement.GetProperty("totalCount").GetInt32());
        var item = Assert.Single(json.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(target.User.Id, item.GetProperty("id").GetString());
        Assert.Equal(AdminFields, item.EnumerateObject().Select(property => property.Name).Order().ToArray());

        using var disable = await client.PatchAsJsonAsync($"/api/v1/users/{target.User.Id}/status", new { isActive = false, reason = "Integration test" }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, disable.StatusCode);
        foreach (var token in new[] { target.RefreshToken, second.RefreshToken })
        {
            using var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = token }).ConfigureAwait(true);
            await AssertProblemAsync(refresh, HttpStatusCode.Forbidden, "AUTH_ACCOUNT_DISABLED").ConfigureAwait(true);
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.False(await db.RefreshTokens.AnyAsync(token => token.UserId == target.User.Id && token.RevokedAt == null).ConfigureAwait(true));
        Assert.True(await db.RefreshTokens.AnyAsync(token => token.UserId == admin.User.Id && token.RevokedAt == null).ConfigureAwait(true));
        using var enable = await client.PatchAsJsonAsync($"/api/v1/users/{target.User.Id}/status", new { isActive = true }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, enable.StatusCode);
        using var oldRefresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { target.RefreshToken }).ConfigureAwait(true);
        await AssertProblemAsync(oldRefresh, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);
        using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { target.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task AdminSearchTreatsSqlWildcardsAsLiteralText()
    {
        using var client = factory.CreateClient();
        var target = await RegisterAsync(client).ConfigureAwait(true);
        var name = $"Cook_%{Guid.NewGuid():N}";
        using var patch = await client.PatchAsJsonAsync("/api/v1/auth/me", new { displayName = name }).ConfigureAwait(true);
        patch.EnsureSuccessStatusCode();
        await RegisterAsync(client, admin: true).ConfigureAwait(true);
        using var response = await client.GetAsync($"/api/v1/users?search={Uri.EscapeDataString(name.ToUpperInvariant())}").ConfigureAwait(true);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        var user = Assert.Single(json.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(target.User.Id, user.GetProperty("id").GetString());
        using var wildcard = await client.GetAsync("/api/v1/users?search=%25").ConfigureAwait(true);
        wildcard.EnsureSuccessStatusCode();
        using var wildcardJson = JsonDocument.Parse(await wildcard.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Single(wildcardJson.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task AdminStatusRejectsSelfDisableMissingUserAndMissingFlag()
    {
        using var client = factory.CreateClient();
        var admin = await RegisterAsync(client, admin: true).ConfigureAwait(true);
        using var self = await client.PatchAsJsonAsync($"/api/v1/users/{admin.User.Id}/status", new { isActive = false }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.Forbidden, self.StatusCode);
        using var missing = await client.PatchAsJsonAsync($"/api/v1/users/{Guid.NewGuid()}/status", new { isActive = false }).ConfigureAwait(true);
        await AssertProblemAsync(missing, HttpStatusCode.NotFound, "AUTH_USER_NOT_FOUND").ConfigureAwait(true);
        using var invalid = await client.PatchAsJsonAsync($"/api/v1/users/{admin.User.Id}/status", new { reason = "Missing flag" }).ConfigureAwait(true);
        await AssertProblemAsync(invalid, HttpStatusCode.BadRequest, "VALIDATION_ERROR").ConfigureAwait(true);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=51")]
    [InlineData("page=2147483647&pageSize=50")]
    public async Task InvalidPagingReturnsBadRequest(string query)
    {
        using var client = factory.CreateClient();
        await RegisterAsync(client, admin: true).ConfigureAwait(true);
        using var response = await client.GetAsync($"/api/v1/users?{query}").ConfigureAwait(true);
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_ERROR").ConfigureAwait(true);
    }

    [Fact]
    public async Task SessionsMarkCurrentAfterRotationAndProtectOwnership()
    {
        using var client = factory.CreateClient();
        var first = await RegisterAsync(client).ConfigureAwait(true);
        var firstSessions = (await client.GetFromJsonAsync<SessionDto[]>("/api/v1/auth/sessions").ConfigureAwait(true))!;
        var firstSession = Assert.Single(firstSessions);
        Assert.True(firstSession.IsCurrent);
        var second = await RegisterAsync(client).ConfigureAwait(true);
        using var foreign = await client.DeleteAsync($"/api/v1/auth/sessions/{firstSession.Id}").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);

        using var rotatedResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { second.RefreshToken }).ConfigureAwait(true);
        rotatedResponse.EnsureSuccessStatusCode();
        var rotated = (await rotatedResponse.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rotated.AccessToken);
        using var list = await client.GetAsync("/api/v1/auth/sessions").ConfigureAwait(true);
        Assert.True(list.Headers.CacheControl?.NoStore);
        using var json = JsonDocument.Parse(await list.Content.ReadAsStringAsync().ConfigureAwait(true));
        var session = Assert.Single(json.RootElement.EnumerateArray());
        Assert.Equal(SessionFields, session.EnumerateObject().Select(property => property.Name).Order().ToArray());
        Assert.True(session.GetProperty("isCurrent").GetBoolean());
        var id = session.GetProperty("id").GetGuid();
        using var revoke = await client.DeleteAsync($"/api/v1/auth/sessions/{id}").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
        using var repeat = await client.DeleteAsync($"/api/v1/auth/sessions/{id}").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.NoContent, repeat.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<SessionDto[]>("/api/v1/auth/sessions").ConfigureAwait(true))!);
        using var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { rotated.RefreshToken }).ConfigureAwait(true);
        await AssertProblemAsync(refresh, HttpStatusCode.Unauthorized, "AUTH_REFRESH_TOKEN_REVOKED").ConfigureAwait(true);
        using var unaffected = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { first.RefreshToken }).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode);
    }

    private async Task<AuthResponseDto> RegisterAsync(HttpClient client, bool admin = false)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            displayName = "Account Test",
            email = $"account-{Guid.NewGuid():N}@example.com",
            password = "TestPassw0rd!",
        }).ConfigureAwait(true);
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        if (admin)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roles.RoleExistsAsync("Admin").ConfigureAwait(true))
            {
                Assert.True((await roles.CreateAsync(new IdentityRole("Admin")).ConfigureAwait(true)).Succeeded);
            }

            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByIdAsync(auth.User.Id).ConfigureAwait(true))!;
            Assert.True((await users.AddToRoleAsync(user, "Admin").ConfigureAwait(true)).Succeeded);
            using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { auth.User.Email, password = "TestPassw0rd!" }).ConfigureAwait(true);
            login.EnsureSuccessStatusCode();
            auth = (await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(true))!;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return auth;
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal(code, json.RootElement.GetProperty("type").GetString());
    }
}
