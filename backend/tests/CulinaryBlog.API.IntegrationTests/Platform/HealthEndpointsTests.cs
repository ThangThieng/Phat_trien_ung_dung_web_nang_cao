using System.Net;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// FR-OBS-001 / SRS §8.7 (Buổi 3 — Dev 4): ba endpoint health chạy trên PostgreSQL, Redis và MinIO THẬT
/// (Testcontainers), và phân biệt đúng "process sống" (live) với "sẵn sàng nhận traffic" (ready).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class HealthEndpointsTests(CulinaryBlogApiFactory factory)
{
    [Fact]
    public async Task Live_Returns200_WithoutRunningAnyCheck()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/health/live", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ready_Returns200_AndChecksOnlyDatabaseAndRedis()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/health/ready", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await ReadEntriesAsync(response);
        Assert.Equal(["database", "redis"], entries.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Health_ReportsEveryDependency()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await ReadEntriesAsync(response);
        Assert.Equal(["database", "minio", "redis"], entries.Keys.Order(StringComparer.Ordinal));
        Assert.All(entries.Values, status => Assert.Equal("Healthy", status));
    }

    [Fact]
    public async Task RedisDown_MakesReadyFail_ButLiveStaysUp()
    {
        // Cùng container PostgreSQL/MinIO, chỉ trỏ Redis tới một cổng không có gì lắng nghe.
        await using var redisDown = factory.WithWebHostBuilder(b => b.UseSetting("ConnectionStrings:Redis", "127.0.0.1:1"));
        var client = redisDown.CreateClient();

        var ready = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));
        var live = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
        Assert.Equal("Unhealthy", (await ReadEntriesAsync(ready))["redis"]);
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
    }

    private static async Task<Dictionary<string, string>> ReadEntriesAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("entries").EnumerateObject()
            .ToDictionary(e => e.Name, e => e.Value.GetProperty("status").GetString()!, StringComparer.Ordinal);
    }
}
