using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.API.IntegrationTests.Infrastructure.Fakes;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Auth;

/// <summary>
/// Buổi 3 — Dev 1: FR-AUTH-003 đăng nhập Google (ID Token flow) với <see cref="FakeGoogleIdTokenValidator"/> (không gọi
/// Google thật), D-1 bộ sinh UserName, FR-AUTH-005 đăng xuất idempotent. Hồi quy khóa 423 nằm ở AuthEndpointsTests.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class GoogleLoginAndLogoutEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Google_NewVerifiedAccount_CreatesAuthorWithRoleAndGoogleLogin()
    {
        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-sub-minh", "minh.tran@gmail.com", true, "Minh Trần", "https://lh3.googleusercontent.com/a/minh"));

        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.ReadAsAsync<AuthResponseDto>();
        Assert.Equal("Minh Trần", auth.User.DisplayName);
        Assert.Contains("Author", auth.User.Roles, StringComparer.Ordinal);
        Assert.False(string.IsNullOrEmpty(auth.RefreshToken));

        // Đăng nhập lại cùng "sub" → đúng tài khoản cũ, không tạo tài khoản thứ hai.
        var again = await (await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token })).ReadAsAsync<AuthResponseDto>();
        Assert.Equal(auth.User.Id, again.User.Id);

        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByLoginAsync("Google", "google-sub-minh");
        Assert.NotNull(user);
        Assert.Equal("minh.tran", user.UserName);
    }

    [Fact]
    public async Task Google_Rejected401_Unavailable502_Malformed400_MissingEmail400()
    {
        var client = factory.CreateClient();

        var rejected = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = FakeGoogleIdTokenValidator.RejectedToken });
        await rejected.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthGoogleTokenInvalid);

        var unavailable = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = FakeGoogleIdTokenValidator.UnavailableToken });
        await unavailable.ShouldBeProblemAsync(HttpStatusCode.BadGateway, ErrorCodes.AuthGoogleUnavailable);

        var malformed = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "khong-phai-jwt" });
        await malformed.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);

        var noEmail = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-no-email", null, false, "Không email", null));
        var missingEmail = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = noEmail });
        await missingEmail.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.AuthGoogleTokenInvalid);
    }

    /// <summary>Email đã có tài khoản nhưng Google CHƯA xác minh email → không liên kết (chống chiếm tài khoản) → 400.</summary>
    [Fact]
    public async Task Google_ExistingEmailNotVerified_Returns400AndDoesNotLink()
    {
        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-attacker", TestDataSeeder.AuthorEmail, false, "Kẻ giả mạo", null));

        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.AuthGoogleTokenInvalid);
        using var scope = factory.Services.CreateScope();
        Assert.Null(await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByLoginAsync("Google", "google-attacker"));
    }

    [Fact]
    public async Task Google_ExistingEmailVerified_LinksToExistingAccount()
    {
        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-author", TestDataSeeder.AuthorEmail, true, "IT Author", null));

        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TestDataSeeder.AuthorUserId, (await response.ReadAsAsync<AuthResponseDto>()).User.Id);
    }

    [Fact]
    public async Task Google_DisabledAccount_Returns403()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var author = (await users.FindByIdAsync(TestDataSeeder.AuthorUserId))!;
            author.IsActive = false;
            await users.UpdateAsync(author);
        }

        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-disabled", TestDataSeeder.AuthorEmail, true, "IT Author", null));
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.AuthAccountDisabled);
    }

    /// <summary>D-1: an.nguyen@x.com rồi an.nguyen@y.com → UserName "an.nguyen" và "an.nguyen2".</summary>
    [Fact]
    public async Task Register_SameLocalPart_GeneratesSuffixedUserName()
    {
        var first = await RegisterAsync("an.nguyen@x.com");
        var second = await RegisterAsync("an.nguyen@y.com");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.Equal("an.nguyen", await db.Users.Where(u => u.Id == first.User.Id).Select(u => u.UserName).SingleAsync());
        Assert.Equal("an.nguyen2", await db.Users.Where(u => u.Id == second.User.Id).Select(u => u.UserName).SingleAsync());
    }

    /// <summary>Hai người đăng ký cùng lúc với cùng tiền tố email → cả hai đều 201 với UserName khác nhau (review mục 10).</summary>
    [Fact]
    public async Task Register_ConcurrentSameLocalPart_BothSucceedWithDistinctUserNames()
    {
        var results = await Task.WhenAll(RegisterAsync("hoa.le@a.com"), RegisterAsync("hoa.le@b.com"), RegisterAsync("hoa.le@c.com"));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var ids = results.Select(r => r.User.Id).ToArray();
        var names = await db.Users.Where(u => ids.Contains(u.Id)).Select(u => u.UserName!).ToListAsync();
        Assert.Equal(["hoa.le", "hoa.le2", "hoa.le3"], names.Order(StringComparer.Ordinal));
    }

    /// <summary>FR-AUTH-005: đăng xuất hai lần liên tiếp đều 204, DB ghi RevokedAt; token lạ vẫn 204 (không lộ token hợp lệ).</summary>
    [Fact]
    public async Task Logout_TwiceAndWithUnknownToken_Returns204AndRevokes()
    {
        var registered = await RegisterAsync($"it-{Guid.NewGuid():N}@culinaryblog.test");
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registered.AccessToken);

        var first = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = registered.RefreshToken });
        var second = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = registered.RefreshToken });
        var unknown = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = "khong-ton-tai" });

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, unknown.StatusCode);

        using var scope = factory.Services.CreateScope();
        var hash = scope.ServiceProvider.GetRequiredService<ITokenService>().HashToken(registered.RefreshToken);
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.NotNull(await db.RefreshTokens.Where(t => t.TokenHash == hash).Select(t => t.RevokedAt).SingleAsync());
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_Returns401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = "bat-ky" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<AuthResponseDto> RegisterAsync(string email)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/register", new
        {
            displayName = "Người dùng kiểm thử",
            email,
            password = TestDataSeeder.ValidPassword,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadAsAsync<AuthResponseDto>();
    }
}
