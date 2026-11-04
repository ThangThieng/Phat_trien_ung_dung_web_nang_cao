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

namespace CulinaryBlog.API.IntegrationTests.Seed;

/// <summary>SRS §2.6.1 (CR-2026-07): seeder chỉ bổ sung, không ghi đè, và công thức Wikibooks chỉ xuất bản qua job nhập ảnh.</summary>
[Collection(IntegrationTestSuite.Name)]
public class DatabaseSeederTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    private const string SeedPassword = "Seed-Test-Password-1!";

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Seed_AddsCatalog_WikibooksAsDraft_LegacyWithoutRandomNutrition()
    {
        var existing = await ExistingSlugsAsync();
        await SeedAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var wikibooksSlugs = WikibooksSlugs();
        var seeded = (await db.Recipes.AsNoTracking().ToListAsync()).Where(r => !existing.Contains(r.Slug)).ToList();

        // Công thức đã có sẵn (dữ liệu của người dùng) không bị ghi đè nên không nằm trong phần vừa thêm.
        var expected = RecipeSeedCatalog.Recipes.Count(r => !existing.Contains(SlugHelper.Generate(r.Title)));
        Assert.Equal(expected, seeded.Count);
        Assert.All(seeded.Where(r => wikibooksSlugs.Contains(r.Slug)), r =>
        {
            Assert.Equal(RecipeStatus.Draft, r.Status);
            Assert.Null(r.PublishedAt);
            Assert.StartsWith(RecipeSeedCatalog.SourceNotePrefix, r.Instructions, StringComparison.Ordinal);
        });

        var legacy = seeded.Where(r => !wikibooksSlugs.Contains(r.Slug)).ToList();
        Assert.Contains(legacy, r => r.Status == RecipeStatus.Published);
        Assert.All(legacy, r => Assert.Null(r.Nutrition));
    }

    [Fact]
    public async Task SeedTwice_DoesNotAddOrOverwrite_UserEditedRecipe()
    {
        await SeedAsync();

        string slug;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var recipe = await db.Recipes.FirstAsync(r => r.Slug == SlugHelper.Generate("Phở bò Hà Nội"));
            slug = recipe.Slug;
            recipe.SetNutrition(new RecipeNutrition { Calories = 123m });
            await db.SaveChangesAsync();
        }

        var countBefore = await CountAsync();
        await SeedAsync();

        Assert.Equal(countBefore, await CountAsync());
        using var verify = factory.Services.CreateScope();
        var check = await verify.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Recipes.AsNoTracking().FirstAsync(r => r.Slug == slug);
        Assert.Equal(123m, check.Nutrition?.Calories);
    }

    /// <summary>
    /// Unique index chỉ áp dụng cho dòng chưa xóa (D-3): một slug có thể có cả bản đã xóa mềm lẫn bản đang hoạt động.
    /// Trước CR-2026-07, <c>ToDictionaryAsync</c> ném lỗi trùng khóa ở tình huống này và API không khởi động được.
    /// </summary>
    [Fact]
    public async Task Seed_DoesNotCrash_WhenSlugHasSoftDeletedAndActiveRow()
    {
        await SeedAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var original = await db.Recipes.FirstAsync(r => r.Slug == SlugHelper.Generate("Phở bò Hà Nội"));
            var replacement = Recipe.Create(original.Title, original.Slug, original.Description, original.CategoryId, original.AuthorId, 1, 0, 1, RecipeDifficulty.Easy);
            original.SoftDelete();
            await db.SaveChangesAsync();
            db.Recipes.Add(replacement);
            await db.SaveChangesAsync();
        }

        var exception = await Record.ExceptionAsync(() => SeedAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task ImportImages_SchedulesEveryRecipeWithImage_AndPublishesOnlyWikibooks()
    {
        var scheduler = new RecordingScheduler();
        var existing = await ExistingSlugsAsync();

        await SeedAsync(importImages: true, scheduler);

        // Công thức của người khác trùng slug với catalog không bao giờ bị gắn ảnh.
        var expected = RecipeSeedCatalog.Recipes.Count(r => r.Image is not null)
            + RecipeSeedCatalog.LegacyImages.Keys.Count(slug => !existing.Contains(slug));
        Assert.Equal(expected, scheduler.Calls.Count);
        Assert.Equal(RecipeSeedCatalog.Recipes.Count(r => r.Image is not null), scheduler.Calls.Count(c => c.PublishAt is not null));
    }

    [Fact]
    public async Task ImportImagesOff_SchedulesNothing()
    {
        var scheduler = new RecordingScheduler();

        await SeedAsync(importImages: false, scheduler);

        Assert.Empty(scheduler.Calls);
    }

    private static HashSet<string> WikibooksSlugs() =>
        RecipeSeedCatalog.Recipes.Where(r => r.Source is not null).Select(r => SlugHelper.Generate(r.Title)).ToHashSet(StringComparer.Ordinal);

    private async Task<HashSet<string>> ExistingSlugsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var slugs = await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Recipes.IgnoreQueryFilters().Select(r => r.Slug).ToListAsync();
        return slugs.ToHashSet(StringComparer.Ordinal);
    }

    private async Task<int> CountAsync()
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Recipes.IgnoreQueryFilters().CountAsync();
    }

    private async Task SeedAsync(bool importImages = false, ISeedImageImportScheduler? scheduler = null)
    {
        using var scope = factory.Services.CreateScope();
        var provider = scope.ServiceProvider;
        var seeder = new DatabaseSeeder(
            provider.GetRequiredService<CulinaryBlogDbContext>(),
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>(),
            Options.Create(new SeedOptions { Enabled = true, AuthorPassword = SeedPassword, ImportImages = importImages }),
            provider.GetRequiredService<ICacheService>(),
            scheduler ?? new RecordingScheduler(),
            TimeProvider.System,
            NullLogger<DatabaseSeeder>.Instance);

        await seeder.SeedAsync(CancellationToken.None);
    }

    private sealed class RecordingScheduler : ISeedImageImportScheduler
    {
        public List<(Guid RecipeId, DateTime? PublishAt)> Calls { get; } = [];

        public void Enqueue(Guid recipeId, DateTime? publishAtUtc) => Calls.Add((recipeId, publishAtUtc));
    }
}
