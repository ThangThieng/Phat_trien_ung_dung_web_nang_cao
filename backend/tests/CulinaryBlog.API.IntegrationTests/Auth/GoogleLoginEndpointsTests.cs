using System.Net;
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
/// Trả nợ kiểm thử Buổi 3 của Dev 1 (bổ sung ở Buổi 4): FR-AUTH-003 Google ID Token flow với
/// <see cref="FakeGoogleIdTokenValidator"/> (không gọi Google thật), D-1 bộ sinh UserName, FR-AUTH-005 đăng xuất idempotent.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class GoogleLoginEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Google_NewVerifiedAccount_CreatesAuthorAndReturns200()
    {
        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-sub-1", "minh.tran@gmail.com", true, "Minh Trần", "https://lh3.googleusercontent.com/a/minh"));

        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.ReadAsAsync<AuthResponseDto>();
        Assert.Equal("Minh Trần", auth.User.DisplayName);
        Assert.Contains("Author", auth.User.Roles, StringComparer.Ordinal);

        // Lần hai cùng "sub" → đăng nhập vào đúng tài khoản cũ, không tạo tài khoản mới.
        var again = await (await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token })).ReadAsAsync<AuthResponseDto>();
        Assert.Equal(auth.User.Id, again.User.Id);
    }

    [Fact]
    public async Task Google_RejectedToken401_Unavailable502_Malformed400()
    {
        var client = factory.CreateClient();

        var rejected = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = FakeGoogleIdTokenValidator.RejectedToken });
        await rejected.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthGoogleTokenInvalid);

        var unavailable = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = FakeGoogleIdTokenValidator.UnavailableToken });
        await unavailable.ShouldBeProblemAsync(HttpStatusCode.BadGateway, ErrorCodes.AuthGoogleUnavailable);

        var malformed = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "khong-phai-jwt" });
        await malformed.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>Email đã có tài khoản nhưng Google CHƯA xác minh email → không liên kết (chống chiếm tài khoản) → 400.</summary>
    [Fact]
    public async Task Google_ExistingEmailNotVerified_Returns400AndDoesNotLink()
    {
        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-attacker", TestDataSeeder.AuthorEmail, false, "Kẻ giả mạo", null));

        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.AuthGoogleTokenInvalid);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByLoginAsync("Google", "google-attacker"));
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
            await scope.ServiceProvider.GetRequiredService<IIdentityService>().SetActiveAsync(TestDataSeeder.AuthorUserId, false, CancellationToken.None);
        }

        var token = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-author-2", TestDataSeeder.AuthorEmail, true, "IT Author", null));
        var response = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/google", new { idToken = token });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.AuthAccountDisabled);
    }

    /// <summary>D-1: an.nguyen@x.com rồi an.nguyen@y.com → UserName "an.nguyen" và "an.nguyen2".</summary>
    [Fact]
    public async Task Register_SameLocalPart_GeneratesSuffixedUserName()
    {
        var first = await AuthApi.RegisterAsync(factory.CreateClient(), email: "an.nguyen@x.com");
        var second = await AuthApi.RegisterAsync(factory.CreateClient(), email: "an.nguyen@y.com");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.Equal("an.nguyen", await db.Users.Where(u => u.Id == first.User.Id).Select(u => u.UserName).SingleAsync());
        Assert.Equal("an.nguyen2", await db.Users.Where(u => u.Id == second.User.Id).Select(u => u.UserName).SingleAsync());
    }

    /// <summary>FR-AUTH-005: đăng xuất hai lần liên tiếp đều 204 và DB ghi RevokedAt.</summary>
    [Fact]
    public async Task Logout_Twice_Returns204AndRevokesToken()
    {
        var registered = await AuthApi.RegisterAsync(factory.CreateClient());
        var client = factory.CreateClient().Authorized(registered.AccessToken);

        var first = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = registered.RefreshToken });
        var second = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = registered.RefreshToken });

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        using var scope = factory.Services.CreateScope();
        var hash = scope.ServiceProvider.GetRequiredService<ITokenService>().HashToken(registered.RefreshToken);
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.NotNull(await db.RefreshTokens.Where(t => t.TokenHash == hash).Select(t => t.RevokedAt).SingleAsync());
    }
}
