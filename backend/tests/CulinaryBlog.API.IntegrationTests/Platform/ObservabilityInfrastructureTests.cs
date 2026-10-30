using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Infrastructure.Caching;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Logging;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Interceptors;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Serilog.Events;
using StackExchange.Redis;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// Buổi 8 — Dev 4: các mắt xích quan sát hệ thống nằm dưới tầng HTTP —
/// <see cref="SlowQueryInterceptor"/> (NFR-PERF-004, &gt; 100 ms, không ghi giá trị tham số), xóa cache theo tiền tố bằng SCAN
/// (audit cache NFR-PERF-003) và <see cref="CorrelationIdJobFilter"/> (CorrelationId đi từ request sang job Hangfire).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class ObservabilityInfrastructureTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task QueryOver100Ms_IsLoggedAsWarningWithSqlButWithoutParameterValues()
    {
        factory.Logs.Clear();
        const string secretParameter = "gia-tri-tham-so-khong-duoc-ghi-log";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();

        await db.Database.ExecuteSqlRawAsync("SELECT pg_sleep(0.15), {0}::text", secretParameter);
        await db.Database.ExecuteSqlRawAsync("SELECT 1");

        var slow = Assert.Single(factory.Logs.Events, e => e.MessageTemplate.Text.StartsWith("Slow query", StringComparison.Ordinal));
        Assert.Equal(LogEventLevel.Warning, slow.Level);
        Assert.Contains("pg_sleep", InMemoryLogSink.Scalar(slow.Properties["CommandText"]), StringComparison.Ordinal);
        Assert.DoesNotContain(secretParameter, InMemoryLogSink.ToJson(slow), StringComparison.Ordinal);
    }

    [Fact]
    public void SensitiveDataLogging_IsDisabled()
    {
        using var scope = factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().GetService<IDbContextOptions>();

        Assert.False(options.FindExtension<CoreOptionsExtension>()?.IsSensitiveDataLoggingEnabled ?? false);
    }

    [Fact]
    public async Task RemoveByPrefix_DeletesWholeFamilyOnly()
    {
        var cache = factory.Services.GetRequiredService<ICacheService>();
        var ttl = TimeSpan.FromMinutes(2);
        await cache.SetAsync("recipes:list:aaa", 1, ttl);
        await cache.SetAsync("recipes:list:bbb", 2, ttl);
        await cache.SetAsync("recipe:pho-bo", 3, ttl);
        await cache.SetAsync("recipes:lists-khac", 4, ttl);

        await cache.RemoveByPrefixAsync(RecipeCacheKeys.ListPrefix);

        Assert.False((await cache.GetAsync<int>("recipes:list:aaa")).Found);
        Assert.False((await cache.GetAsync<int>("recipes:list:bbb")).Found);
        Assert.True((await cache.GetAsync<int>("recipe:pho-bo")).Found);
        Assert.True((await cache.GetAsync<int>("recipes:lists-khac")).Found);
    }

    /// <summary>
    /// NFR-REL-002 dưới tải: Redis không tới được thì GET/SET là cache-miss NGAY và REMOVE bỏ qua ngay, không chờ hết asyncTimeout
    /// 2 giây mỗi lệnh (đo trước khi sửa: tắt Redis giữa lượt k6 100 người dùng → mỗi request đọc ~5,6 giây, p95 lượt ghi 4,19 s).
    /// </summary>
    [Fact]
    public async Task RedisUnreachable_GetSetAndRemoveReturnImmediately()
    {
        using var unreachable = await ConnectionMultiplexer.ConnectAsync("127.0.0.1:1,abortConnect=false,connectTimeout=200,asyncTimeout=2000");
        var options = Options.Create(new RedisCacheOptions
        {
            InstanceName = "culinaryblog:",
            ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(unreachable),
        });
        using var distributed = new RedisCache(options);
        var cache = new RedisCacheService(distributed, unreachable, options, TimeProvider.System, NullLogger<RedisCacheService>.Instance);

        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 5; i++)
        {
            Assert.False((await cache.GetAsync<int>("recipe:pho-bo")).Found);
            await cache.SetAsync("recipe:pho-bo", 1, TimeSpan.FromMinutes(5));
            await cache.RemoveAsync("recipe:pho-bo");
        }

        Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(500), $"5 lượt GET/SET/REMOVE mất {watch.ElapsedMilliseconds} ms.");
    }

    /// <summary>
    /// Ngắt mạch sau một lần lỗi (Redis còn "kết nối" nhưng lệnh lỗi/timeout): 10 giây sau đó GET/SET không gọi Redis, hết hạn thì thử
    /// lại; REMOVE không bao giờ bị ngắt — bỏ một lệnh xóa lúc Redis vừa hồi phục là để dữ liệu cũ sống tới hết TTL.
    /// </summary>
    [Fact]
    public async Task AfterAFailure_GetAndSetSkipRedisForTenSeconds_RemoveIsNeverSkipped()
    {
        var connected = factory.Services.GetRequiredService<IConnectionMultiplexer>();
        var failing = new FailingDistributedCache();
        var clock = new ManualTimeProvider(DateTimeOffset.Parse("2026-10-30T17:00:00Z", System.Globalization.CultureInfo.InvariantCulture));
        var cache = new RedisCacheService(
            failing, connected, Options.Create(new RedisCacheOptions { InstanceName = "culinaryblog:" }), clock, NullLogger<RedisCacheService>.Instance);

        Assert.False((await cache.GetAsync<int>("recipe:a")).Found);
        Assert.Equal(1, failing.Calls);

        clock.Advance(TimeSpan.FromSeconds(9));
        Assert.False((await cache.GetAsync<int>("recipe:a")).Found);
        await cache.SetAsync("recipe:a", 1, TimeSpan.FromMinutes(5));
        Assert.Equal(1, failing.Calls);

        await cache.RemoveAsync("recipe:a");
        Assert.Equal(2, failing.Calls);

        clock.Advance(TimeSpan.FromSeconds(2));
        Assert.False((await cache.GetAsync<int>("recipe:a")).Found);
        Assert.Equal(3, failing.Calls);
    }

    /// <summary>
    /// Hồi quy lỗi tìm ra ở lượt audit cache: trước Buổi 8 "xóa theo prefix <c>recipes:list:</c>" xóa một khóa không tồn tại, nên sửa
    /// một công thức đã xuất bản thì danh sách công khai vẫn trả tiêu đề cũ tới hết TTL 2 phút.
    /// </summary>
    [Fact]
    public async Task UpdatingPublishedRecipe_RefreshesCachedPublicListImmediately()
    {
        var guest = factory.CreateClient();
        var author = factory.CreateClientAs("Author");
        var before = await guest.GetFromJsonAsync<PagedResult<RecipeSummaryDto>>("/api/v1/recipes?pageSize=50", HttpAssertions.Json);
        var target = Assert.Single(before!.Items, r => r.Slug == TestDataSeeder.PublishedRecipeSlug);

        var mine = await author.GetAsync(new Uri($"/api/v1/recipes/mine/{target.Id}", UriKind.Relative));
        var etag = mine.Headers.ETag!.Tag.Trim('"');
        var newTitle = $"Phở bò Hà Nội — bản sửa {Guid.NewGuid():N}"[..48];
        var update = await author.PutAsJsonAsync($"/api/v1/recipes/{target.Id}", new { title = newTitle, rowVersion = etag });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var after = await guest.GetFromJsonAsync<PagedResult<RecipeSummaryDto>>("/api/v1/recipes?pageSize=50", HttpAssertions.Json);
        Assert.Equal(newTitle, Assert.Single(after!.Items, r => r.Id == target.Id).Title);
    }

    /// <summary>
    /// CR-2026-06 (MT-69): sửa một BẢN NHÁP không chạm tập Published nên không được xóa danh sách công khai đang cache — đo Buổi 8:
    /// khi còn xóa, 10 % lượt ghi lên bản nháp kéo hit rate từ 97,9 % xuống 75,4 %.
    /// </summary>
    [Fact]
    public async Task UpdatingDraftRecipe_KeepsCachedPublicListAndCategoryPages()
    {
        var guest = factory.CreateClient();
        var author = factory.CreateClientAs("Author");
        Assert.Equal(HttpStatusCode.OK, (await guest.GetAsync(new Uri("/api/v1/recipes?pageSize=7", UriKind.Relative))).StatusCode);
        Assert.Equal(1, await CountRedisKeysAsync(RecipeCacheKeys.ListPrefix));

        var drafts = await author.GetFromJsonAsync<PagedResult<RecipeSummaryDto>>("/api/v1/recipes/mine?status=Draft", HttpAssertions.Json);
        var draft = Assert.Single(drafts!.Items, r => r.Slug == TestDataSeeder.DraftRecipeSlug);
        var mine = await author.GetAsync(new Uri($"/api/v1/recipes/mine/{draft.Id}", UriKind.Relative));
        var update = await author.PutAsJsonAsync(
            $"/api/v1/recipes/{draft.Id}",
            new { description = "Bản nháp sửa trong lúc danh sách công khai đang được cache.", rowVersion = mine.Headers.ETag!.Tag.Trim('"') });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Assert.Equal(1, await CountRedisKeysAsync(RecipeCacheKeys.ListPrefix));
    }

    [Fact]
    public async Task CorrelationId_TravelsFromEnqueueToJobExecutionLogs()
    {
        factory.Logs.Clear();
        var storage = factory.Services.GetRequiredService<JobStorage>();
        var correlationId = $"it-job-{Guid.NewGuid():N}";

        // Phía client (container api): enqueue trong ngữ cảnh của một request.
        string jobId;
        using (CorrelationContext.Begin(correlationId))
        {
            jobId = new BackgroundJobClient(storage).Enqueue<WelcomeEmailJob>(job => job.ExecuteAsync(Guid.Empty.ToString(), CancellationToken.None));
        }

        using var connection = storage.GetConnection();
        Assert.Equal($"\"{correlationId}\"", connection.GetJobParameter(jobId, CorrelationContext.PropertyName));

        // Phía server (container hangfire): filter đặt lại ngữ cảnh trước khi job chạy, log của job mang mã đó.
        var data = connection.GetJobData(jobId);
        var background = new BackgroundJob(jobId, data.Job, data.CreatedAt);
        var perform = new PerformContext(storage, connection, background, new JobCancellationToken(false));
        var filter = new CorrelationIdJobFilter();

        Assert.Null(CorrelationContext.Current);
        filter.OnPerforming(new PerformingContext(perform));
        Assert.Equal(correlationId, CorrelationContext.Current);
#pragma warning disable CA1848 // log thử trong "thân job"
        factory.Services.GetRequiredService<ILogger<ObservabilityInfrastructureTests>>().LogInformation("Inside job body");
#pragma warning restore CA1848
        filter.OnPerformed(new PerformedContext(perform, null, false, null));

        Assert.Null(CorrelationContext.Current);
        var jobLog = Assert.Single(factory.Logs.Events, e => e.MessageTemplate.Text == "Inside job body");
        Assert.Equal(correlationId, InMemoryLogSink.Scalar(jobLog.Properties["CorrelationId"]));
        Assert.Equal(jobId, InMemoryLogSink.Scalar(jobLog.Properties["JobId"]));
        Assert.Equal(nameof(WelcomeEmailJob), InMemoryLogSink.Scalar(jobLog.Properties["JobType"]));
    }

    [Fact]
    public void JobWithoutCorrelationId_GetsAFreshOne_AndHostsDoNotDuplicateTheGlobalFilter()
    {
        Assert.Single(GlobalJobFilters.Filters, f => f.Instance is CorrelationIdJobFilter);
        Assert.False(CorrelationContext.IsValid(null));
        Assert.False(CorrelationContext.IsValid(new string('x', CorrelationContext.MaxLength + 1)));
        Assert.True(CorrelationContext.IsValid(new string('x', CorrelationContext.MaxLength)));
        Assert.True(CorrelationContext.IsValid(CorrelationContext.NewId()));
    }

    private async Task<int> CountRedisKeysAsync(string prefix)
    {
        var redis = factory.Services.GetRequiredService<IConnectionMultiplexer>();
        return await redis.GetServer(redis.GetEndPoints()[0]).KeysAsync(pattern: $"culinaryblog:{prefix}*").CountAsync();
    }

    private sealed class FailingDistributedCache : IDistributedCache
    {
        public int Calls { get; private set; }

        public byte[]? Get(string key) => throw Fail();

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => throw Fail();

        public void Refresh(string key) => throw Fail();

        public Task RefreshAsync(string key, CancellationToken token = default) => throw Fail();

        public void Remove(string key) => throw Fail();

        public Task RemoveAsync(string key, CancellationToken token = default) => throw Fail();

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Fail();

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw Fail();

        private RedisTimeoutException Fail()
        {
            Calls++;
            return new RedisTimeoutException("giả lập Redis timeout", CommandStatus.Sent);
        }
    }

    private sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
