using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Auth;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.API.Logging;
using CulinaryBlog.Application.Features.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// Buổi 8 — Dev 4: NFR-SEC-007 "raw refresh token, password, <c>Authorization</c> header, Google <c>idToken</c> và token đặt lại
/// mật khẩu không bao giờ được đưa vào log ở bất kỳ sink nào". Gọi THẬT các luồng mang bí mật qua API rồi đọc mọi log event mà
/// sink nhận (sink trong bộ nhớ nhận đúng những gì Console/File/Seq nhận, sau bước che) — chuỗi bí mật nào xuất hiện là đỏ.
/// Lời hứa "không log bí mật" trở thành điều kiện build.
///
/// <c>change-password</c> và <c>reset-password</c> (FR-AUTH-010/011) do Dev 1 làm ở Buổi 7 — khi nhánh đó vào <c>develop</c>, thêm
/// hai luồng vào <see cref="AuthFlows_NeverWriteSecretsToAnySink"/> theo cùng mẫu.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class LogRedactionTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AuthFlows_NeverWriteSecretsToAnySink()
    {
        factory.Logs.Clear();
        var secrets = new List<string>();
        var client = factory.CreateClient();
        var email = $"redaction-{Guid.NewGuid():N}@culinaryblog.test";

        // Đăng ký + đăng nhập: mật khẩu trong body; response mang access token và refresh token.
        var registered = await AuthApi.RegisterAsync(client, email);
        var loggedIn = await AuthApi.LoginAsync(client, email);
        secrets.AddRange([TestDataSeeder.ValidPassword, registered.AccessToken, registered.RefreshToken, loggedIn.AccessToken, loggedIn.RefreshToken]);

        // Đăng nhập sai (mật khẩu khác) — nhánh lỗi cũng không được ghi mật khẩu.
        const string wrongPassword = "Sai#MatKhau-B8-2026";
        secrets.Add(wrongPassword);
        var failed = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = wrongPassword });
        Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);

        // Refresh: refresh token cũ trong body, cặp token mới trong response.
        var refreshed = await (await AuthApi.RefreshAsync(client, loggedIn.RefreshToken)).ReadAsAsync<AuthResponseDto>();
        secrets.AddRange([refreshed.AccessToken, refreshed.RefreshToken]);

        // Header Authorization: Bearer <access token> trên một request đã xác thực.
        using (var me = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me"))
        {
            me.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
            Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(me)).StatusCode);
        }

        // Google ID token (validator giả, token vẫn là chuỗi bí mật client gửi lên).
        var idToken = factory.GoogleTokens.Issue(new GoogleIdTokenPayload("google-sub-redaction", "redaction.b8@gmail.com", true, "Kiểm Thử Log", null));
        secrets.Add(idToken);
        var google = await (await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken })).ReadAsAsync<AuthResponseDto>();
        secrets.AddRange([google.AccessToken, google.RefreshToken]);

        // Đăng xuất: refresh token trong body, access token trong header.
        var logout = await factory.CreateClient().Authorized(google.AccessToken)
            .PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = google.RefreshToken });
        Assert.True(logout.IsSuccessStatusCode, $"Đăng xuất trả {(int)logout.StatusCode}.");

        await Task.Delay(500); // dòng hoàn tất của request logging ghi sau khi response đã về client.
        var events = factory.Logs.Events;

        // Đối chứng: các luồng trên THẬT SỰ sinh log (audit của từng command), nếu không bài kiểm tra vô nghĩa.
        foreach (var command in new[] { "RegisterUserCommand", "LoginUserCommand", "RefreshTokenCommand", "GoogleLoginCommand", "LogoutCommand" })
        {
            Assert.Contains(events, e => e.MessageTemplate.Text.StartsWith("AUDIT ", StringComparison.Ordinal)
                && InMemoryLogSink.Scalar(e.Properties["Command"]) == command);
        }

        var leaks = (
            from e in events
            let json = InMemoryLogSink.ToJson(e)
            from secret in secrets.Distinct(StringComparer.Ordinal)
            where json.Contains(secret, StringComparison.Ordinal)
            select $"{e.MessageTemplate.Text} chứa bí mật dài {secret.Length} ký tự").ToList();
        Assert.True(leaks.Count == 0, string.Join(Environment.NewLine, leaks));
    }

    [Fact]
    public void LogStatementsNamingASecret_AreMaskedBeforeReachingTheSink()
    {
        factory.Logs.Clear();
        var logger = factory.Services.GetRequiredService<ILogger<LogRedactionTests>>();
        const string token = "refresh-token-khong-duoc-ghi-ra-sink-0123456789";
        const string password = "MatKhau#KhongDuocGhi-2026";

#pragma warning disable CA1848, CA1873, CA2254 // Cố ý ghi log kiểu "lỡ tay" để kiểm lớp chặn cuối.
        logger.LogInformation("Refreshing with {RefreshToken} for {Email}", token, "a@b.c");
        logger.LogInformation("Command payload {@Command}", new { Email = "a@b.c", Password = password, CurrentPassword = password });
        using (logger.BeginScope(new Dictionary<string, object> { ["Authorization"] = "Bearer " + token }))
        {
            logger.LogInformation("Inside a scope carrying the Authorization header");
        }
#pragma warning restore CA1848, CA1873, CA2254

        var json = string.Join('\n', factory.Logs.Events.Select(InMemoryLogSink.ToJson));
        Assert.DoesNotContain(token, json, StringComparison.Ordinal);
        Assert.DoesNotContain(password, json, StringComparison.Ordinal);
        Assert.Contains(SensitiveData.Mask, json, StringComparison.Ordinal);
        Assert.Contains("a@b.c", json, StringComparison.Ordinal); // thuộc tính thường vẫn giữ nguyên.
    }
}
