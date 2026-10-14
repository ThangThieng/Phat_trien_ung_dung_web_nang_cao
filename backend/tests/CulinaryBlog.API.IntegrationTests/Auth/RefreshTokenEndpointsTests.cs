using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Auth;

/// <summary>
/// Buổi 4 — Dev 1: FR-AUTH-004 Token Rotation + Reuse Detection, D-2 (256-bit), claim sid và UseForwardedHeaders —
/// kiểm chứng trên PostgreSQL thật (bảng RefreshTokens: RevokedAt, ReplacedByTokenHash, CreatedByIp).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RefreshTokenEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Refresh_WithValidToken_IssuesNewPairAndRevokesOldWithReplacedByHash()
    {
        var client = factory.CreateClient();
        var registered = await AuthApi.RegisterAsync(client);

        var response = await AuthApi.RefreshAsync(client, registered.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rotated = await response.ReadAsAsync<AuthResponseDto>();
        Assert.NotEqual(registered.RefreshToken, rotated.RefreshToken);
        Assert.NotEqual(registered.AccessToken, rotated.AccessToken);
        Assert.Equal(registered.User.Id, rotated.User.Id);

        var (oldToken, newToken) = await LoadTokensAsync(registered.RefreshToken, rotated.RefreshToken);
        Assert.NotNull(oldToken.RevokedAt);
        Assert.Equal(newToken.TokenHash, oldToken.ReplacedByTokenHash); // MT-15: lưu HASH của token mới, không phải token gốc
        Assert.Null(newToken.RevokedAt);

        // sid của access token mới = Id bản ghi RefreshToken mới (FR-AUTH-009).
        Assert.Equal(newToken.Id, AuthApi.SessionIdOf(rotated.AccessToken));
    }

    /// <summary>D-2 (MT-13): refresh token 256-bit = 32 byte → 43 ký tự Base64URL không padding.</summary>
    [Fact]
    public async Task IssuedRefreshToken_Is256Bit()
    {
        var registered = await AuthApi.RegisterAsync(factory.CreateClient());

        Assert.Equal(43, registered.RefreshToken.Length);
    }

    /// <summary>FR-AUTH-004 A3: dùng lại token cũ sau khi đã xoay vòng → thu hồi CẢ họ token + 401 REVOKED.</summary>
    [Fact]
    public async Task Refresh_ReusingRotatedToken_RevokesWholeFamilyAndReturns401Revoked()
    {
        var client = factory.CreateClient();
        var registered = await AuthApi.RegisterAsync(client);
        var secondDevice = await AuthApi.LoginAsync(client, registered.User.Email);
        var rotated = await (await AuthApi.RefreshAsync(client, registered.RefreshToken)).ReadAsAsync<AuthResponseDto>();

        var reuse = await AuthApi.RefreshAsync(client, registered.RefreshToken);
        await reuse.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthRefreshTokenRevoked);

        // Token mới nhất của kẻ trộm/nạn nhân và phiên ở thiết bị khác đều đã chết.
        var afterReuse = await AuthApi.RefreshAsync(client, rotated.RefreshToken);
        await afterReuse.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthRefreshTokenRevoked);
        var otherDevice = await AuthApi.RefreshAsync(client, secondDevice.RefreshToken);
        await otherDevice.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthRefreshTokenRevoked);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.False(await db.RefreshTokens.AnyAsync(t => t.UserId == registered.User.Id && t.RevokedAt == null));
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_Returns401Expired()
    {
        var client = factory.CreateClient();
        var registered = await AuthApi.RegisterAsync(client);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var hash = scope.ServiceProvider.GetRequiredService<ITokenService>().HashToken(registered.RefreshToken);
            var past = DateTime.UtcNow.AddMinutes(-1);
            await db.Database.ExecuteSqlAsync($"UPDATE \"RefreshTokens\" SET \"ExpiresAt\" = {past} WHERE \"TokenHash\" = {hash}");
        }

        var response = await AuthApi.RefreshAsync(client, registered.RefreshToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthRefreshTokenExpired);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_Returns401Invalid_AndMissingToken400()
    {
        var client = factory.CreateClient();

        var unknown = await AuthApi.RefreshAsync(client, "khong-ton-tai-trong-db");
        await unknown.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthTokenInvalid);

        var missing = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { });
        await missing.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>MT-38: request đi qua Nginx (mạng tin cậy) kèm X-Forwarded-For → CreatedByIp ghi IP thật của người dùng.</summary>
    [Fact]
    public async Task Refresh_ThroughTrustedProxy_RecordsClientIpFromForwardedFor()
    {
        var client = factory.CreateClient();
        var registered = await AuthApi.RegisterAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh")
        {
            Content = JsonContent.Create(new { refreshToken = registered.RefreshToken }),
        };
        request.Headers.Add("X-Forwarded-For", "203.0.113.25");
        var response = await client.SendAsync(request);
        var rotated = await response.ReadAsAsync<AuthResponseDto>();

        var (oldToken, newToken) = await LoadTokensAsync(registered.RefreshToken, rotated.RefreshToken);
        Assert.Equal("203.0.113.25", newToken.CreatedByIp);
        Assert.Equal(SimulatedProxyStartupFilter.NginxAddress.ToString(), oldToken.CreatedByIp); // không có header → IP của proxy
    }

    private async Task<(Domain.Entities.RefreshToken Old, Domain.Entities.RefreshToken New)> LoadTokensAsync(string oldRaw, string newRaw)
    {
        using var scope = factory.Services.CreateScope();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var oldHash = tokens.HashToken(oldRaw);
        var newHash = tokens.HashToken(newRaw);
        return (
            await db.RefreshTokens.AsNoTracking().SingleAsync(t => t.TokenHash == oldHash),
            await db.RefreshTokens.AsNoTracking().SingleAsync(t => t.TokenHash == newHash));
    }
}
