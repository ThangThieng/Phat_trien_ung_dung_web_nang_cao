using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Auth;

/// <summary>
/// Buổi 4 — Dev 1: FR-AUTH-006/007 hồ sơ, FR-AUTH-008 quản lý tài khoản [Admin], FR-AUTH-009 quản lý phiên — kiểm chứng
/// hợp đồng SRS §8.1: không lộ trường nhạy cảm, no-store, 403/404 đúng chỗ, khóa tài khoản thì phiên chết ngay.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class AccountEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    private static readonly string[] SensitiveFields =
        ["passwordHash", "securityStamp", "concurrencyStamp", "userName", "emailConfirmed", "tokenHash"];

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- FR-AUTH-006 / 007 ----------
    [Fact]
    public async Task GetMe_ReturnsProfileWithoutSensitiveFields_AndNoStore()
    {
        var registered = await AuthApi.RegisterAsync(factory.CreateClient(), displayName: "Lan Anh");
        var client = factory.CreateClient().Authorized(registered.AccessToken);

        var response = await client.GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore, "GET /auth/me phải trả Cache-Control: no-store.");
        var json = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveFields(json);
        var me = JsonSerializer.Deserialize<UserDto>(json, HttpAssertions.Json)!;
        Assert.Equal("Lan Anh", me.DisplayName);
        Assert.Contains("Author", me.Roles, StringComparer.Ordinal);
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PatchMe_UpdatesOnlySentFields()
    {
        var registered = await AuthApi.RegisterAsync(factory.CreateClient(), displayName: "Tên cũ");
        var client = factory.CreateClient().Authorized(registered.AccessToken);
        var avatarUrl = $"{factory.MinioEndpoint}/culinary-blog-tests/uploads/{registered.User.Id}/avatar.png";

        var first = await client.PatchAsJsonAsync("/api/v1/auth/me", new { bio = "Đầu bếp nghiệp dư ở Đà Lạt.", avatarUrl });
        var afterFirst = await first.ReadAsAsync<UserDto>();
        var second = await client.PatchAsJsonAsync("/api/v1/auth/me", new { displayName = "Tên mới" });
        var afterSecond = await second.ReadAsAsync<UserDto>();

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal("Tên cũ", afterFirst.DisplayName);
        Assert.Equal(avatarUrl, afterFirst.AvatarUrl);
        Assert.Equal("Tên mới", afterSecond.DisplayName);
        Assert.Equal("Đầu bếp nghiệp dư ở Đà Lạt.", afterSecond.Bio);
        Assert.Equal(registered.User.Email, afterSecond.Email);
    }

    /// <summary>FR-AUTH-007: avatarUrl phải là tệp đã tải lên hệ thống — ảnh ở domain ngoài bị từ chối.</summary>
    [Fact]
    public async Task PatchMe_WithExternalAvatarUrl_Returns400()
    {
        var registered = await AuthApi.RegisterAsync(factory.CreateClient());
        var client = factory.CreateClient().Authorized(registered.AccessToken);

        var response = await client.PatchAsJsonAsync("/api/v1/auth/me", new { avatarUrl = "https://evil.example.com/tracker.png" });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("avatarUrl", (await response.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);
    }

    [Fact]
    public async Task PatchMe_WithTooShortDisplayName_Returns400()
    {
        var registered = await AuthApi.RegisterAsync(factory.CreateClient());
        var client = factory.CreateClient().Authorized(registered.AccessToken);

        var response = await client.PatchAsJsonAsync("/api/v1/auth/me", new { displayName = "A" });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    // ---------- FR-AUTH-008 ----------
    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsPagedAdminDtos_AndNoStore()
    {
        await AuthApi.RegisterAsync(factory.CreateClient(), email: "tim-kiem-lan@culinaryblog.test", displayName: "Lan Tìm Kiếm");
        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);

        var response = await admin.GetAsync(new Uri("/api/v1/users?search=tim-kiem&pageSize=5", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore, "GET /users phải trả Cache-Control: no-store.");
        var json = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveFields(json);
        var page = JsonSerializer.Deserialize<PagedResult<UserAdminDto>>(json, HttpAssertions.Json)!;
        var user = Assert.Single(page.Items);
        Assert.Equal("Lan Tìm Kiếm", user.DisplayName);
        Assert.True(user.IsActive);
        Assert.Equal(["Author"], user.Roles);

        // recipeCount: tác giả của bộ dữ liệu test có 4 công thức.
        var authors = await (await admin.GetAsync(new Uri("/api/v1/users?search=it-author@", UriKind.Relative))).ReadAsAsync<PagedResult<UserAdminDto>>();
        Assert.Equal(4, Assert.Single(authors.Items).RecipeCount);
    }

    [Fact]
    public async Task GetUsers_AsAuthor_Returns403_AndInvalidPageSize400()
    {
        var author = factory.CreateClientAs("Author");
        var forbidden = await author.GetAsync(new Uri("/api/v1/users", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);
        var invalid = await admin.GetAsync(new Uri("/api/v1/users?pageSize=51", UriKind.Relative));
        await invalid.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>Khóa tài khoản → mọi refresh token bị thu hồi; refresh bằng token cũ → 403 DISABLED; đăng nhập lại → 403 DISABLED.</summary>
    [Fact]
    public async Task Deactivate_RevokesSessions_AndBlocksLogin()
    {
        var victim = await AuthApi.RegisterAsync(factory.CreateClient());
        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);

        var response = await admin.PatchAsJsonAsync($"/api/v1/users/{victim.User.Id}/status", new { isActive = false, reason = "Spam" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var status = await response.ReadAsAsync<UserStatusDto>();
        Assert.False(status.IsActive);

        // Kế hoạch Buổi 4 — Dev 1 bước 9: refresh token của người bị khóa → 403 AUTH_ACCOUNT_DISABLED (không phải reuse 401).
        var refresh = await AuthApi.RefreshAsync(factory.CreateClient(), victim.RefreshToken);
        await refresh.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.AuthAccountDisabled);

        var login = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { email = victim.User.Email, password = TestDataSeeder.ValidPassword });
        await login.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.AuthAccountDisabled);
    }

    /// <summary>FR-AUTH-004 A4: tài khoản bị khóa SAU khi token được cấp (token chưa bị thu hồi) → refresh trả 403 DISABLED.</summary>
    [Fact]
    public async Task Refresh_ForDeactivatedAccountWithStillValidToken_Returns403Disabled()
    {
        var victim = await AuthApi.RegisterAsync(factory.CreateClient());
        using (var scope = factory.Services.CreateScope())
        {
            // Khóa trực tiếp qua IIdentityService (không qua endpoint) để token vẫn còn hiệu lực — mô phỏng nhánh A4.
            await scope.ServiceProvider.GetRequiredService<IIdentityService>().SetActiveAsync(victim.User.Id, false, CancellationToken.None);
        }

        var refresh = await AuthApi.RefreshAsync(factory.CreateClient(), victim.RefreshToken);

        await refresh.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.AuthAccountDisabled);
    }

    [Fact]
    public async Task Reactivate_AllowsLoginAgain()
    {
        var victim = await AuthApi.RegisterAsync(factory.CreateClient());
        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);
        await admin.PatchAsJsonAsync($"/api/v1/users/{victim.User.Id}/status", new { isActive = false });

        var reactivate = await admin.PatchAsJsonAsync($"/api/v1/users/{victim.User.Id}/status", new { isActive = true, reason = "Đã xác minh" });

        Assert.Equal(HttpStatusCode.OK, reactivate.StatusCode);
        Assert.True((await reactivate.ReadAsAsync<UserStatusDto>()).IsActive);
        var login = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { email = victim.User.Email, password = TestDataSeeder.ValidPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task SetStatus_OnSelf_Returns403_UnknownUser404_MissingIsActive400()
    {
        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);

        var self = await admin.PatchAsJsonAsync($"/api/v1/users/{TestDataSeeder.AdminUserId}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.Forbidden, self.StatusCode);

        var unknown = await admin.PatchAsJsonAsync($"/api/v1/users/{Guid.NewGuid()}/status", new { isActive = false });
        await unknown.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.AuthUserNotFound);

        var missing = await admin.PatchAsJsonAsync($"/api/v1/users/{TestDataSeeder.AuthorUserId}/status", new { reason = "Quên isActive" });
        await missing.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    // ---------- FR-AUTH-009 ----------
    [Fact]
    public async Task GetSessions_MarksCurrentSessionBySid_AndNeverExposesTokenHash()
    {
        var anonymous = factory.CreateClient();
        var first = await AuthApi.RegisterAsync(anonymous);
        var second = await AuthApi.LoginAsync(anonymous, first.User.Email);
        var client = factory.CreateClient().Authorized(second.AccessToken);

        var response = await client.GetAsync(new Uri("/api/v1/auth/sessions", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore, "GET /auth/sessions phải trả Cache-Control: no-store.");
        var json = await response.Content.ReadAsStringAsync();
        AssertNoSensitiveFields(json);
        var sessions = JsonSerializer.Deserialize<List<SessionDto>>(json, HttpAssertions.Json)!;
        Assert.Equal(2, sessions.Count);
        var current = Assert.Single(sessions, s => s.IsCurrent);
        Assert.Equal(AuthApi.SessionIdOf(second.AccessToken), current.Id);
    }

    /// <summary>A1: phiên của người khác → 404 (không phải 403) để không dò được id; phiên đó vẫn sống.</summary>
    [Fact]
    public async Task RevokeSession_OfAnotherUser_Returns404AndKeepsIt()
    {
        var alice = await AuthApi.RegisterAsync(factory.CreateClient());
        var bob = await AuthApi.RegisterAsync(factory.CreateClient());
        var bobSessionId = AuthApi.SessionIdOf(bob.AccessToken);

        var response = await factory.CreateClient().Authorized(alice.AccessToken)
            .DeleteAsync(new Uri($"/api/v1/auth/sessions/{bobSessionId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthApi.RefreshAsync(factory.CreateClient(), bob.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task RevokeSession_Own_Returns204Twice_AndTokenStopsWorking()
    {
        var anonymous = factory.CreateClient();
        var phone = await AuthApi.RegisterAsync(anonymous);
        var laptop = await AuthApi.LoginAsync(anonymous, phone.User.Email);
        var client = factory.CreateClient().Authorized(laptop.AccessToken);
        var phoneSession = new Uri($"/api/v1/auth/sessions/{AuthApi.SessionIdOf(phone.AccessToken)}", UriKind.Relative);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(phoneSession)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(phoneSession)).StatusCode); // A2: idempotent

        // Phiên laptop không bị ảnh hưởng. Kiểm tra laptop TRƯỚC: dùng lại token điện thoại đã bị thu hồi là đúng kịch bản
        // reuse detection (FR-AUTH-004 A3) — cả họ token của người dùng sẽ bị thu hồi, kể cả laptop.
        Assert.Equal(HttpStatusCode.OK, (await AuthApi.RefreshAsync(anonymous, laptop.RefreshToken)).StatusCode);
        await (await AuthApi.RefreshAsync(anonymous, phone.RefreshToken))
            .ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthRefreshTokenRevoked);
    }

    [Fact]
    public async Task RevokeAll_KillsEverySessionIncludingCurrent()
    {
        var anonymous = factory.CreateClient();
        var phone = await AuthApi.RegisterAsync(anonymous);
        var laptop = await AuthApi.LoginAsync(anonymous, phone.User.Email);

        var response = await factory.CreateClient().Authorized(laptop.AccessToken)
            .PostAsync(new Uri("/api/v1/auth/sessions/revoke-all", UriKind.Relative), content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthApi.RefreshAsync(anonymous, phone.RefreshToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await AuthApi.RefreshAsync(anonymous, laptop.RefreshToken)).StatusCode);
    }

    /// <summary>Ba endpoint phiên đều yêu cầu Bearer — không có access token → 401 (NFR-MAINT-002: luồng lỗi cho mọi endpoint).</summary>
    [Fact]
    public async Task SessionEndpoints_WithoutToken_Return401()
    {
        var anonymous = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(new Uri("/api/v1/auth/sessions", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.DeleteAsync(new Uri($"/api/v1/auth/sessions/{Guid.NewGuid()}", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync(new Uri("/api/v1/auth/sessions/revoke-all", UriKind.Relative), content: null)).StatusCode);
    }

    private static void AssertNoSensitiveFields(string json)
    {
        foreach (var field in SensitiveFields)
        {
            Assert.DoesNotContain($"\"{field}\"", json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
