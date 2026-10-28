using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CulinaryBlog.Application.UnitTests;

/// <summary>
/// Buổi 8 — Dev 4: audit trong <c>LoggingBehavior</c> (FR-OBS-002, NFR-SEC-006/007) và xóa cache theo tiền tố trong
/// <c>CacheInvalidationBehavior</c> (audit cache, NFR-PERF-003).
/// </summary>
public class ObservabilityBehaviorTests
{
    private const string Secret = "MatKhau#KhongDuocGhiLog-2026";

    [Fact]
    public async Task Command_Success_IsAuditedWithUserIdAndWithoutPayload()
    {
        var logger = new RecordingLogger<LoggingBehavior<SampleCommand, string>>();
        var behavior = new LoggingBehavior<SampleCommand, string>(logger, CurrentUser("user-42"));

        await behavior.Handle(new SampleCommand(Secret), _ => Task.FromResult("ok"), CancellationToken.None);

        var audit = Assert.Single(logger.Entries, e => e.Message.StartsWith("AUDIT ", StringComparison.Ordinal));
        Assert.Equal(LogLevel.Information, audit.Level);
        Assert.Equal("AUDIT SampleCommand by user-42 succeeded in " + audit.State["ElapsedMs"] + " ms", audit.Message);
        Assert.DoesNotContain(logger.Entries, e => e.Message.Contains(Secret, StringComparison.Ordinal)
            || e.State.Values.Any(v => v?.ToString()?.Contains(Secret, StringComparison.Ordinal) == true));
    }

    [Fact]
    public async Task Command_Failure_IsAuditedAsWarningWithErrorCode_AndRethrown()
    {
        var logger = new RecordingLogger<LoggingBehavior<SampleCommand, string>>();
        var behavior = new LoggingBehavior<SampleCommand, string>(logger, CurrentUser(null));
        var failure = new BusinessRuleViolationException(ErrorCodes.RecipePublishIncomplete, "Thiếu bước.");

        var thrown = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            behavior.Handle(new SampleCommand(Secret), _ => throw failure, CancellationToken.None));

        Assert.Same(failure, thrown);
        var audit = Assert.Single(logger.Entries, e => e.Message.StartsWith("AUDIT ", StringComparison.Ordinal));
        Assert.Equal(LogLevel.Warning, audit.Level);
        Assert.Equal(AuditLog.AnonymousUser, audit.State["UserId"]);
        Assert.Equal(ErrorCodes.RecipePublishIncomplete, audit.State["ErrorCode"]);
    }

    [Fact]
    public async Task Query_IsLoggedAsHandled_NotAudited()
    {
        var logger = new RecordingLogger<LoggingBehavior<SampleQuery, int>>();
        var behavior = new LoggingBehavior<SampleQuery, int>(logger, CurrentUser("user-42"));

        await behavior.Handle(new SampleQuery(), _ => Task.FromResult(1), CancellationToken.None);

        Assert.DoesNotContain(logger.Entries, e => e.Message.StartsWith("AUDIT ", StringComparison.Ordinal));
        Assert.Contains(logger.Entries, e => e.Message.StartsWith("Handled SampleQuery", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SlowCommand_AlsoWarnsAboveFiveHundredMilliseconds()
    {
        var logger = new RecordingLogger<LoggingBehavior<SampleCommand, string>>();
        var behavior = new LoggingBehavior<SampleCommand, string>(logger, CurrentUser("user-42"));

        await behavior.Handle(
            new SampleCommand("x"),
            async ct =>
            {
                await Task.Delay(520, ct);
                return "ok";
            },
            CancellationToken.None);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Message.StartsWith("Slow request SampleCommand", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Invalidation_PrefixKeysUseScan_ExactKeysUseRemove()
    {
        var cache = Substitute.For<ICacheService>();
        var behavior = new CacheInvalidationBehavior<InvalidatingCommand, Unit>(cache);
        var command = new InvalidatingCommand(["recipe:pho-bo", "recipes:list:", "categories:detail:", "recipes:list:"]);

        await behavior.Handle(command, _ => Task.FromResult(Unit.Value), CancellationToken.None);

        await cache.Received(1).RemoveAsync("recipe:pho-bo", Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveByPrefixAsync("recipes:list:", Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveByPrefixAsync("categories:detail:", Arg.Any<CancellationToken>());
        await cache.DidNotReceive().RemoveAsync("recipes:list:", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invalidation_DoesNothingWhenHandlerFails()
    {
        var cache = Substitute.For<ICacheService>();
        var behavior = new CacheInvalidationBehavior<InvalidatingCommand, Unit>(cache);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new InvalidatingCommand(["recipes:list:"]), _ => throw new InvalidOperationException(), CancellationToken.None));

        await cache.DidNotReceiveWithAnyArgs().RemoveByPrefixAsync(default!, default);
    }

    /// <summary>Đối chiếu cột "Invalidate khi" của bảng TTL chuẩn NFR-PERF-003 sau CR-2026-06 (bảng đầy đủ: docs/cache-report.md).</summary>
    [Fact]
    public void RecipeKeySets_MatchSrsInvalidationColumn()
    {
        var published = RecipeCacheKeys.ForContentChange("pho-bo", RecipeStatus.Published);
        Assert.Equal(["recipe:pho-bo", "recipes:list:", "categories:detail:"], published);

        var visibility = RecipeCacheKeys.ForVisibilityChange("pho-bo", RecipeStatus.Draft, RecipeStatus.Published);
        Assert.Equal(["recipe:pho-bo", "recipes:list:", "recipes:sitemap", "categories:detail:", CategoryCacheKeys.All], visibility);
        Assert.Equal(visibility, RecipeCacheKeys.ForVisibilityChange("pho-bo", RecipeStatus.Published, RecipeStatus.Archived));
        Assert.Equal([CategoryCacheKeys.All], RecipeCacheKeys.ForCategoryChange(RecipeStatus.Published));

        // search:{queryHash} — "Không invalidate, để hết hạn tự nhiên" (TTL 1 phút).
        Assert.DoesNotContain(published.Concat(visibility), k => k.StartsWith(RecipeCacheKeys.SearchPrefix, StringComparison.Ordinal));
    }

    /// <summary>CR-2026-06 (MT-69): thay đổi không chạm tập Published không xóa họ khóa công khai — chỉ xóa chi tiết.</summary>
    [Theory]
    [InlineData(RecipeStatus.Draft)]
    [InlineData(RecipeStatus.Archived)]
    public void ChangesOutsideThePublishedSet_OnlyEvictTheDetailKey(RecipeStatus status)
    {
        Assert.Equal(["recipe:nhap"], RecipeCacheKeys.ForContentChange("nhap", status));
        Assert.Empty(RecipeCacheKeys.ForCategoryChange(status));
        Assert.Equal(["recipe:nhap"], RecipeCacheKeys.ForVisibilityChange("nhap", status, status)); // xóa mềm bản nháp/lưu trữ
        Assert.Equal(["recipe:nhap"], RecipeCacheKeys.ForVisibilityChange("nhap", RecipeStatus.Archived, RecipeStatus.Draft)); // khôi phục
        Assert.Equal(["recipe:nhap"], RecipeCacheKeys.ForVisibilityChange("nhap", RecipeStatus.Draft, RecipeStatus.Archived)); // lưu trữ nháp
    }

    private static ICurrentUser CurrentUser(string? userId)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        return user;
    }

    public sealed record SampleCommand(string Password) : IRequest<string>;

    public sealed record SampleQuery : IRequest<int>;

    public sealed record InvalidatingCommand(IReadOnlyCollection<string> CacheKeysToInvalidate) : IRequest<Unit>, ICacheInvalidator;

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<Entry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            var values = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
            Entries.Add(new Entry(logLevel, formatter(state, exception), values.ToDictionary(p => p.Key, p => p.Value)));
        }
    }

    public sealed record Entry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> State);
}
