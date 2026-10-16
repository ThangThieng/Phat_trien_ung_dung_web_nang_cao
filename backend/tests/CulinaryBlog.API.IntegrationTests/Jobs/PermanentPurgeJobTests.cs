using Amazon.S3;
using CulinaryBlog.API.IntegrationTests.Content;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;

namespace CulinaryBlog.API.IntegrationTests.Jobs;

/// <summary>
/// Buổi 4 — Dev 4: FR-JOB-003 Permanent Purge Job trên PostgreSQL + MinIO + Redis thật. Chạy thân job trực tiếp
/// (không qua Hangfire server) để khẳng định kết quả tất định: xóa đúng bản ghi quá 30 ngày cùng tệp của nó,
/// giữ nguyên bản ghi mới xóa, và nhường lượt khi khóa phân tán đang bị giữ.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class PermanentPurgeJobTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Purge_DeletesRecipesSoftDeletedOver30DaysAgo_AndTheirFiles()
    {
        var author = factory.CreateClientAs("Author");
        var expired = await CreateSoftDeletedRecipeAsync(author, "Bò kho bánh mì", deletedDaysAgo: 40);
        var recent = await CreateSoftDeletedRecipeAsync(author, "Cá kho tộ", deletedDaysAgo: 10);

        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<PermanentPurgeJob>().PurgeAsync(CancellationToken.None);

        Assert.Equal(1, result.PurgedRecipes);
        Assert.Equal(1, result.DeletedFiles);

        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.False(await db.Recipes.IgnoreQueryFilters().AnyAsync(r => r.Id == expired.RecipeId));
        Assert.False(await db.RecipeImages.IgnoreQueryFilters().AnyAsync(i => i.RecipeId == expired.RecipeId), "FK CASCADE phải dọn ảnh.");
        Assert.True(await db.Recipes.IgnoreQueryFilters().AnyAsync(r => r.Id == recent.RecipeId), "Công thức mới xóa 10 ngày phải còn.");

        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        Assert.False(await MinioObject.ExistsAsync(s3, expired.ImageUrl), "Tệp của công thức đã dọn phải bị xóa khỏi MinIO.");
        Assert.True(await MinioObject.ExistsAsync(s3, recent.ImageUrl), "Tệp của công thức còn trong hạn khôi phục phải còn.");
    }

    /// <summary>Công thức CHƯA xóa mềm (dù rất cũ) không bao giờ bị dọn.</summary>
    [Fact]
    public async Task Purge_IgnoresRecipesThatAreNotSoftDeleted()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var old = DateTime.UtcNow.AddDays(-90);
        await db.Database.ExecuteSqlAsync($"UPDATE \"Recipes\" SET \"UpdatedAt\" = {old}");
        var before = await db.Recipes.CountAsync();

        var result = await scope.ServiceProvider.GetRequiredService<PermanentPurgeJob>().PurgeAsync(CancellationToken.None);

        Assert.Equal(0, result.PurgedRecipes);
        Assert.Equal(before, await db.Recipes.CountAsync());
    }

    /// <summary>NFR-SCALE-001: một lượt khác đang giữ "lock:purge-job" → lượt này bỏ qua, không xóa gì.</summary>
    [Fact]
    public async Task Execute_WhenLockIsHeldByAnotherRun_SkipsWithoutDeleting()
    {
        var author = factory.CreateClientAs("Author");
        var expired = await CreateSoftDeletedRecipeAsync(author, "Thịt kho trứng", deletedDaysAgo: 45);

        var lockFactory = factory.Services.GetRequiredService<IDistributedLockFactory>();
        await using (var held = await lockFactory.CreateLockAsync(PermanentPurgeJob.LockKey, TimeSpan.FromMinutes(1)))
        {
            Assert.True(held.IsAcquired);

            using var scope = factory.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<PermanentPurgeJob>().ExecuteAsync(CancellationToken.None);

            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            Assert.True(await db.Recipes.IgnoreQueryFilters().AnyAsync(r => r.Id == expired.RecipeId));
        }

        // Khóa đã nhả → lượt kế tiếp dọn được.
        using (var scope = factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<PermanentPurgeJob>().ExecuteAsync(CancellationToken.None);
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            Assert.False(await db.Recipes.IgnoreQueryFilters().AnyAsync(r => r.Id == expired.RecipeId));
        }
    }

    /// <summary>Tạo công thức + một ảnh qua API, xóa mềm qua API, rồi lùi UpdatedAt về quá khứ (AuditInterceptor luôn gán "bây giờ").</summary>
    private async Task<(Guid RecipeId, string ImageUrl)> CreateSoftDeletedRecipeAsync(HttpClient author, string title, int deletedDaysAgo)
    {
        var recipe = await RecipeApi.CreateDraftAsync(author, title);
        var image = await RecipeApi.UploadImageAsync(author, recipe.Id, TestImages.Png(), "image/png", "anh.png");
        var deleted = await author.DeleteAsync(new Uri($"/api/v1/recipes/{recipe.Id}", UriKind.Relative));
        deleted.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var deletedAt = DateTime.UtcNow.AddDays(-deletedDaysAgo);
        await db.Database.ExecuteSqlAsync($"UPDATE \"Recipes\" SET \"UpdatedAt\" = {deletedAt} WHERE \"Id\" = {recipe.Id}");

        return (recipe.Id, image.OriginalUrl);
    }
}
