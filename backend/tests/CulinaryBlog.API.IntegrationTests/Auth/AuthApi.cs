using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Features.Auth;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CulinaryBlog.API.IntegrationTests.Auth;

/// <summary>Dựng người dùng/phiên THẬT qua chính API (đăng ký → token thật có claim sid) cho các lớp test module Auth.</summary>
internal static class AuthApi
{
    public static async Task<AuthResponseDto> RegisterAsync(HttpClient client, string? email = null, string displayName = "Người dùng kiểm thử")
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            displayName,
            email = email ?? $"it-{Guid.NewGuid():N}@culinaryblog.test",
            password = TestDataSeeder.ValidPassword,
        });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"Đăng ký thất bại ({(int)response.StatusCode}): {body}");
        return await response.ReadAsAsync<AuthResponseDto>();
    }

    public static async Task<AuthResponseDto> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = TestDataSeeder.ValidPassword });
        response.EnsureSuccessStatusCode();
        return await response.ReadAsAsync<AuthResponseDto>();
    }

    public static Task<HttpResponseMessage> RefreshAsync(HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken });

    public static HttpClient Authorized(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static Guid SessionIdOf(string accessToken) =>
        Guid.Parse(new JsonWebTokenHandler().ReadJsonWebToken(accessToken).GetClaim("sid").Value);
}
