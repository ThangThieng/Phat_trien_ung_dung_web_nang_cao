using System.Net;
using System.Net.Http.Headers;
using Amazon.S3;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CulinaryBlog.API.IntegrationTests.Jobs;

/// <summary>
/// CR-2026-07: job nhập ảnh trên PostgreSQL và MinIO thật, chỉ thay phần tải từ Wikimedia bằng HttpMessageHandler giả
/// (test không gọi mạng ngoài). Ảnh JPEG sinh bằng ImageSharp trong test.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class SeedImageImportJobTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    private static readonly DateTime PublishAt = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

    private static SeedRecipe WikibooksRecipe => RecipeSeedCatalog.Recipes.First(r => r.Source is not null);

    private static string WikibooksSlug => SlugHelper.Generate(WikibooksRecipe.Title);

    /// <summary>Slug không trùng bộ dữ liệu test, nên công thức này do seeder tạo.</summary>
    private static string LegacySlug => SlugHelper.Generate("Bánh khọt Vũng Tàu");

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        using var scope = factory.Services.CreateScope();
        var provider = scope.ServiceProvider;
        var seeder = new DatabaseSeeder(
            provider.GetRequiredService<CulinaryBlogDbContext>(),
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>(),
            Options.Create(new SeedOptions { Enabled = true, AuthorPassword = "Seed-Test-Password-1!" }),
            provider.GetRequiredService<ICacheService>(),
            provider.GetRequiredService<ISeedImageImportScheduler>(),
            TimeProvider.System,
            NullLogger<DatabaseSeeder>.Instance);
        await seeder.SeedAsync(CancellationToken.None);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ValidImage_IsStoredInMinio_ThenRecipeIsPublished_AndResizeQueued()
    {
        factory.BackgroundJobs.Clear();
        var recipe = await RecipeAsync(WikibooksSlug);

        await RunJobAsync(recipe.Id, PublishAt, Respond(HttpStatusCode.OK, JpegPhoto(), "image/jpeg"));

        var updated = await RecipeAsync(WikibooksSlug);
        Assert.Equal(RecipeStatus.Published, updated.Status);
        Assert.Equal(PublishAt, updated.PublishedAt);

        var image = Assert.Single(updated.Images);
        Assert.True(image.IsPrimary);
        Assert.Equal(updated.Title, image.AltText);
        Assert.Contains($"/recipes/{updated.Id}/", image.OriginalUrl, StringComparison.Ordinal);
        Assert.True(factory.BackgroundJobs.WasEnqueued<ImageResizeJob>(nameof(ImageResizeJob.ExecuteAsync), image.Id));

        using var scope = factory.Services.CreateScope();
        Assert.True(await MinioObject.ExistsAsync(scope.ServiceProvider.GetRequiredService<IAmazonS3>(), image.OriginalUrl));
    }

    [Fact]
    public async Task NetworkError_IsRethrownForRetry_AndRecipeStaysDraft()
    {
        var recipe = await RecipeAsync(WikibooksSlug);

        await Assert.ThrowsAsync<HttpRequestException>(() => RunJobAsync(recipe.Id, PublishAt, Respond(HttpStatusCode.TooManyRequests)));

        var updated = await RecipeAsync(WikibooksSlug);
        Assert.Equal(RecipeStatus.Draft, updated.Status);
        Assert.Null(updated.PublishedAt);
        Assert.Empty(updated.Images);
    }

    [Fact]
    public async Task RateLimited_WaitsForRetryAfter_ThenImports()
    {
        var recipe = await RecipeAsync(WikibooksSlug);
        var calls = 0;
        var image = Respond(HttpStatusCode.OK, JpegPhoto(), "image/jpeg");
        var started = System.Diagnostics.Stopwatch.StartNew();

        await RunJobAsync(recipe.Id, PublishAt, request =>
        {
            if (++calls > 1)
            {
                return image(request);
            }

            var limited = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            limited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
            return limited;
        });

        Assert.Equal(2, calls);
        Assert.True(started.Elapsed >= TimeSpan.FromSeconds(1), $"Chưa chờ đủ Retry-After: {started.Elapsed}");
        Assert.Equal(RecipeStatus.Published, (await RecipeAsync(WikibooksSlug)).Status);
    }

    [Fact]
    public async Task RetryAfterTooLong_IsLeftToHangfire_WithoutSecondRequest()
    {
        var recipe = await RecipeAsync(WikibooksSlug);
        var calls = 0;

        await Assert.ThrowsAsync<HttpRequestException>(() => RunJobAsync(recipe.Id, PublishAt, _ =>
        {
            calls++;
            var limited = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            limited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromHours(1));
            return limited;
        }));

        Assert.Equal(1, calls);
        Assert.Equal(RecipeStatus.Draft, (await RecipeAsync(WikibooksSlug)).Status);
    }

    [Fact]
    public void Job_RunsOnItsOwnQueue_AndIdentifiesItselfToWikimedia()
    {
        var method = typeof(SeedImageImportJob).GetMethod(nameof(SeedImageImportJob.ExecuteAsync))!;
        var queue = Assert.Single(method.GetCustomAttributes(typeof(Hangfire.QueueAttribute), inherit: false));
        Assert.Equal(SeedImageImportJob.QueueName, ((Hangfire.QueueAttribute)queue).Queue);

        using var client = factory.Services.GetRequiredService<IHttpClientFactory>().CreateClient(SeedImageImportJob.HttpClientName);
        var userAgent = string.Join(' ', client.DefaultRequestHeaders.GetValues("User-Agent"));
        Assert.Contains("Bot/", userAgent, StringComparison.Ordinal);
        Assert.Contains("(https://", userAgent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NonImageFile_IsRejected_WithoutRetry_AndRecipeStaysDraft()
    {
        var recipe = await RecipeAsync(WikibooksSlug);

        await RunJobAsync(recipe.Id, PublishAt, Respond(HttpStatusCode.OK, "<html>không phải ảnh</html>"u8.ToArray(), "image/jpeg"));

        var updated = await RecipeAsync(WikibooksSlug);
        Assert.Equal(RecipeStatus.Draft, updated.Status);
        Assert.Empty(updated.Images);
    }

    [Fact]
    public async Task RunningTwice_DoesNotAddSecondImage()
    {
        var recipe = await RecipeAsync(WikibooksSlug);
        var photo = JpegPhoto();

        await RunJobAsync(recipe.Id, PublishAt, Respond(HttpStatusCode.OK, photo, "image/jpeg"));
        await RunJobAsync(recipe.Id, PublishAt, Respond(HttpStatusCode.OK, photo, "image/jpeg"));

        Assert.Single((await RecipeAsync(WikibooksSlug)).Images);
    }

    [Fact]
    public async Task LegacyRecipe_GetsImageAndCredit_WithoutChangingStatus()
    {
        var before = await RecipeAsync(LegacySlug);

        await RunJobAsync(before.Id, publishAtUtc: null, Respond(HttpStatusCode.OK, JpegPhoto(), "image/jpeg"));

        var after = await RecipeAsync(LegacySlug);
        Assert.Equal(before.Status, after.Status);
        Assert.Single(after.Images);
        Assert.Equal(RecipeSeedCatalog.LegacyImageCredit(RecipeSeedCatalog.LegacyImages[LegacySlug]), after.Instructions);
    }

    [Fact]
    public async Task EditedLegacyRecipe_IsLeftUntouched()
    {
        Guid id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var recipe = await db.Recipes.FirstAsync(r => r.Slug == LegacySlug);
            id = recipe.Id;
            db.Entry(recipe).Property(r => r.Instructions).CurrentValue = "Ghi chú do người dùng tự viết.";
            await db.SaveChangesAsync();
        }

        await RunJobAsync(id, publishAtUtc: null, Respond(HttpStatusCode.OK, JpegPhoto(), "image/jpeg"));

        var after = await RecipeAsync(LegacySlug);
        Assert.Empty(after.Images);
        Assert.Equal("Ghi chú do người dùng tự viết.", after.Instructions);
    }

    [Fact]
    public async Task DeletedRecipe_IsIgnored()
    {
        var exception = await Record.ExceptionAsync(() => RunJobAsync(Guid.NewGuid(), PublishAt, Respond(HttpStatusCode.OK, JpegPhoto(), "image/jpeg")));

        Assert.Null(exception);
    }

    private static Func<HttpRequestMessage, HttpResponseMessage> Respond(HttpStatusCode status, byte[]? body = null, string? contentType = null) => _ =>
    {
        var response = new HttpResponseMessage(status);
        if (body is not null)
        {
            response.Content = new ByteArrayContent(body);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType!);
        }

        return response;
    };

    private static byte[] JpegPhoto()
    {
        using var image = new Image<Rgb24>(1000, 800, new Rgb24(200, 120, 60));
        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        return stream.ToArray();
    }

    private async Task<Recipe> RecipeAsync(string slug)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Recipes
            .AsNoTracking().Include(r => r.Images).FirstAsync(r => r.Slug == slug);
    }

    private async Task RunJobAsync(Guid recipeId, DateTime? publishAtUtc, Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        using var scope = factory.Services.CreateScope();
        var provider = scope.ServiceProvider;
        var job = new SeedImageImportJob(
            provider.GetRequiredService<CulinaryBlogDbContext>(),
            new FakeHttpClientFactory(responder),
            provider.GetRequiredService<IFileStorageService>(),
            provider.GetRequiredService<IImageResizeScheduler>(),
            provider.GetRequiredService<ICacheService>(),
            Options.Create(new SeedOptions { ImageImportPaceSeconds = 0, ImageImportRateLimitWaitSeconds = 0 }),
            NullLogger<SeedImageImportJob>.Instance);

        await job.ExecuteAsync(recipeId, publishAtUtc, CancellationToken.None);
    }

    private sealed class FakeHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> responder) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new StubHandler(responder));

        private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(responder(request));
        }
    }
}
