using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Domain.Exceptions;
using Serilog.Events;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// Buổi 8 — Dev 4: FR-OBS-002, CONS-010, SRS §5.2 "Correlation ID". Mỗi request một CorrelationId: nhận từ header khi hợp lệ
/// (≤ 64 ký tự <c>[A-Za-z0-9-]</c>), ngược lại sinh mới; trả lại trong header + Problem Details; MỌI log của request mang
/// <c>CorrelationId</c>, <c>RequestPath</c> và <c>UserId</c> (khi đã xác thực).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class CorrelationIdTests(CulinaryBlogApiFactory factory)
{
    private const string Header = "X-Correlation-ID";

    [Fact]
    public async Task ValidIncomingId_IsEchoedInResponseHeader()
    {
        var id = $"it-{Guid.NewGuid():N}";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        request.Headers.Add(Header, id);

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, Assert.Single(response.Headers.GetValues(Header)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("chua khoang trang")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("a;DROP")]
    public async Task MissingOrUnsafeId_IsReplacedByNewUuid(string incoming)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        if (incoming.Length > 0)
        {
            request.Headers.TryAddWithoutValidation(Header, incoming);
        }

        var response = await factory.CreateClient().SendAsync(request);

        var returned = Assert.Single(response.Headers.GetValues(Header));
        Assert.NotEqual(incoming, returned);
        Assert.True(Guid.TryParse(returned, out _), $"Mã sinh mới phải là UUID, nhận '{returned}'.");
    }

    [Fact]
    public async Task OverlongId_IsReplaced_SoClientsCannotFloodTheLog()
    {
        var overlong = new string('a', 65);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories");
        request.Headers.Add(Header, overlong);

        var response = await factory.CreateClient().SendAsync(request);

        Assert.NotEqual(overlong, Assert.Single(response.Headers.GetValues(Header)));
        Assert.DoesNotContain(factory.Logs.Events, e => InMemoryLogSink.ToJson(e).Contains(overlong, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProblemDetails_CarryTheSameCorrelationIdAsTheHeader()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/khong-ton-tai-b8", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var header = Assert.Single(response.Headers.GetValues(Header));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(header, body.RootElement.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task InternalError_DetailContainsOnlyGenericMessageAndCorrelationId()
    {
        var response = await factory.CreateClient().GetAsync(new Uri(ErrorTriggerStartupFilter.PathPrefix + "unhandled", UriKind.Relative));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var header = Assert.Single(response.Headers.GetValues(Header));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(header, body.RootElement.GetProperty("detail").GetString(), StringComparison.Ordinal);
        Assert.Equal(header, body.RootElement.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task EveryLogOfAnAuthenticatedRequest_HasCorrelationIdRequestPathAndUserId()
    {
        var id = $"it-{Guid.NewGuid():N}";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/recipes/mine");
        request.Headers.Add(Header, id);

        var response = await factory.CreateClientAs("Author").SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var completion = await WaitForAsync(id, e => e.MessageTemplate.Text.StartsWith("HTTP ", StringComparison.Ordinal));
        var events = factory.Logs.ForCorrelation(id);

        // Dòng log của LoggingBehavior (query) + dòng request logging — tối thiểu hai tầng khác nhau cùng mang mã.
        Assert.Contains(events, e => e.MessageTemplate.Text.StartsWith("Handled ", StringComparison.Ordinal));
        Assert.All(events, e =>
        {
            Assert.Equal("/api/v1/recipes/mine", InMemoryLogSink.Scalar(e.Properties["RequestPath"]));
            Assert.Equal(TestDataSeeder.AuthorUserId, InMemoryLogSink.Scalar(e.Properties["UserId"]));
            Assert.Equal("CulinaryBlog.API", InMemoryLogSink.Scalar(e.Properties["Application"]));
        });

        Assert.Equal(LogEventLevel.Information, completion.Level);
        Assert.Equal("200", InMemoryLogSink.Scalar(completion.Properties["StatusCode"]));
        Assert.Equal("GET", InMemoryLogSink.Scalar(completion.Properties["RequestMethod"]));
        Assert.True(completion.Properties.ContainsKey("Elapsed"));
        Assert.True(completion.Properties.ContainsKey("ClientIp"));
        Assert.Equal("1", InMemoryLogSink.Scalar(completion.Properties["ApiVersion"])); // /api/v1/… → phiên bản 1
    }

    [Fact]
    public async Task AnonymousRequest_LogsHaveNoUserId_AndCommandsAreAuditedAsAnonymous()
    {
        var id = $"it-{Guid.NewGuid():N}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(new { email = "khong-ton-tai-b8@culinaryblog.test", password = "Sai#Matkhau2026" }),
        };
        request.Headers.Add(Header, id);

        var response = await factory.CreateClient().SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        await WaitForAsync(id, e => e.MessageTemplate.Text.StartsWith("HTTP ", StringComparison.Ordinal));
        var events = factory.Logs.ForCorrelation(id);

        var audit = Assert.Single(events, e => e.MessageTemplate.Text.StartsWith("AUDIT ", StringComparison.Ordinal));

        // Chưa xác thực: không log nào có UserId thật — chỉ dòng audit ghi rõ "anonymous".
        Assert.All(events.Where(e => e != audit), e => Assert.False(e.Properties.ContainsKey("UserId")));
        Assert.Equal(LogEventLevel.Warning, audit.Level);
        Assert.Equal("LoginUserCommand", InMemoryLogSink.Scalar(audit.Properties["Command"]));
        Assert.Equal(AuditLog.AnonymousUser, InMemoryLogSink.Scalar(audit.Properties["UserId"]));
        Assert.Equal(ErrorCodes.AuthInvalidCredentials, InMemoryLogSink.Scalar(audit.Properties["ErrorCode"]));
    }

    [Fact]
    public async Task HealthChecks_AreLoggedAtVerbose_SoDockerProbesDoNotFloodTheLog()
    {
        var id = $"it-{Guid.NewGuid():N}";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add(Header, id);

        await factory.CreateClient().SendAsync(request);

        // Mức tối thiểu ở môi trường test là Debug → sự kiện Verbose bị lọc trước khi tới sink.
        await Task.Delay(300);
        Assert.DoesNotContain(factory.Logs.ForCorrelation(id), e => e.MessageTemplate.Text.StartsWith("HTTP ", StringComparison.Ordinal));
    }

    /// <summary>
    /// Request logging ghi dòng hoàn tất SAU khi pipeline trả về — TestServer có thể giao response cho client trước đó vài mili giây.
    /// </summary>
    private async Task<LogEvent> WaitForAsync(string correlationId, Func<LogEvent, bool> predicate)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var match = factory.Logs.ForCorrelation(correlationId).FirstOrDefault(predicate);
            if (match is not null)
            {
                return match;
            }

            await Task.Delay(50);
        }

        throw new Xunit.Sdk.XunitException($"Không thấy log mong đợi cho CorrelationId {correlationId}.");
    }
}
