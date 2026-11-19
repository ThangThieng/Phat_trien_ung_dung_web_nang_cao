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
    public async Task Seed_CreatesSampleAuthorsWhoseNamesSayTheyAreSamples()
    {
        await SeedAsync();

        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        for (var i = 1; i <= 5; i++)
        {
            var author = await users.FindByEmailAsync($"author{i}@culinaryblog.local");
            Assert.NotNull(author);
            Assert.Equal($"Tác giả mẫu {i}", author.DisplayName);
            Assert.Equal(RecipeSeedCatalog.SampleAuthorBio, author.Bio);
        }
    }

    /// <summary>
    /// Database dựng trước 19/11/2026 có tên do Bogus đặt (seed 2026): author1 = "Kim Quyên Vũ" (đo trên database dev). Seed lại thì
    /// đổi sang tên mẫu; hồ sơ người dùng đã tự sửa (FR-AUTH-007) giữ nguyên.
    /// </summary>
    [Fact]
    public async Task Reseed_RenamesLegacyRandomNames_ButKeepsProfilesUsersEdited()
    {
        await SeedAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var legacy = await users.FindByEmailAsync("author1@culinaryblog.local");
            legacy!.DisplayName = "Kim Quyên Vũ";
            legacy.Bio = "Đầu bếp tại gia, đam mê ẩm thực miền Nam.";
            await users.UpdateAsync(legacy);
            var edited = await users.FindByEmailAsync("author2@culinaryblog.local");
            edited!.DisplayName = "Tên người dùng tự sửa";
            edited.Bio = "Giới thiệu tự viết";
            await users.UpdateAsync(edited);
        }

        await SeedAsync();

        using var verify = factory.Services.CreateScope();
        var check = verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var renamed = await check.FindByEmailAsync("author1@culinaryblog.local");
        Assert.Equal("Tác giả mẫu 1", renamed!.DisplayName);
        Assert.Equal(RecipeSeedCatalog.SampleAuthorBio, renamed.Bio);
        var kept = await check.FindByEmailAsync("author2@culinaryblog.local");
        Assert.Equal("Tên người dùng tự sửa", kept!.DisplayName);
        Assert.Equal("Giới thiệu tự viết", kept.Bio);
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

    /// <summary>
    /// Database seed trước 19/11/2026 còn bản dịch cũ (kể cả tên cũ "Bánh cuốn" của món Dürüm). Seed lại: đưa bản sửa vào, đổi tên kèm
    /// slug mà không thêm công thức trùng; giá trị người dùng đã tự sửa giữ nguyên.
    /// </summary>
    [Fact]
    public async Task Reseed_AppliesTranslationCorrections_RenamesWithoutDuplicate_KeepsUserEdits()
    {
        await SeedAsync();
        const string durumOld = "Dürüm bữa sáng (Bánh cuốn trứng, khoai và bò)";
        var durumNew = CatalogRecipe(480539).Title;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var durum = await RecipeBySourceAsync(db, 480539);
            await db.Recipes.Where(r => r.Id == durum)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Title, durumOld).SetProperty(r => r.Slug, SlugHelper.Generate(durumOld)));
            var haggis = await RecipeBySourceAsync(db, 31180);
            await db.Set<RecipeIngredient>().Where(i => i.RecipeId == haggis && i.Name == "ruột bò làm vỏ dồi (beef caps)")
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Name, "màng bọc dạ dày bò (beef caps)"));
            var chowMein = await RecipeBySourceAsync(db, 156848);
            await db.Set<RecipeStep>().Where(st => st.RecipeId == chowMein && st.Description.Contains("Cho hành tây tím và seitan vào"))
                .ExecuteUpdateAsync(s => s.SetProperty(st => st.Description, st => st.Description.Replace("Cho hành tây tím và seitan vào", "Cho hành tím và seitan vào")));
            var frijoles = await RecipeBySourceAsync(db, 127705);
            await db.Set<RecipeIngredient>().Where(i => i.RecipeId == frijoles && i.Name == "thịt nạm bò, giò heo hoặc da heo")
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Name, "thịt do người dùng tự ghi"));
        }

        var countBefore = await CountAsync();
        await SeedAsync();

        Assert.Equal(countBefore, await CountAsync());
        using var verify = factory.Services.CreateScope();
        var check = verify.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var slugs = await check.Recipes.IgnoreQueryFilters().Select(r => r.Slug).ToListAsync();
        Assert.Contains(SlugHelper.Generate(durumNew), slugs);
        Assert.DoesNotContain(SlugHelper.Generate(durumOld), slugs);
        var haggisId = await RecipeBySourceAsync(check, 31180);
        Assert.True(await check.Set<RecipeIngredient>().AnyAsync(i => i.RecipeId == haggisId && i.Name == "ruột bò làm vỏ dồi (beef caps)"));
        var chowMeinId = await RecipeBySourceAsync(check, 156848);
        Assert.True(await check.Set<RecipeStep>().AnyAsync(st => st.RecipeId == chowMeinId && st.Description.Contains("Cho hành tây tím và seitan vào")));
        var frijolesId = await RecipeBySourceAsync(check, 127705);
        Assert.True(await check.Set<RecipeIngredient>().AnyAsync(i => i.RecipeId == frijolesId && i.Name == "thịt do người dùng tự ghi"));
    }

    /// <summary>Mọi chỗ sửa khớp catalog đã sửa: database mới (seed từ JSON) và database cũ (seed + sửa) cho cùng một kết quả.</summary>
    [Fact]
    public void TextCorrections_MatchTheCorrectedCatalog()
    {
        foreach (var fix in SeedTextCorrections.All)
        {
            var recipe = CatalogRecipe(fix.PageId);
            string[] texts = fix.Field switch
            {
                SeedTextField.Title => [recipe.Title],
                SeedTextField.Description => [recipe.Description],
                SeedTextField.IngredientName => recipe.Ingredients.Select(i => i.Name).ToArray(),
                _ => recipe.Steps.Select(s => s.Description).ToArray(),
            };
            Assert.Contains(texts, t => fix.Field is SeedTextField.Title or SeedTextField.IngredientName ? t == fix.New : t.Contains(fix.New, StringComparison.Ordinal));
            Assert.DoesNotContain(texts, t => t.Contains(fix.Old, StringComparison.Ordinal));
        }
    }

    private static SeedRecipe CatalogRecipe(long pageId) => RecipeSeedCatalog.Recipes.Single(r => r.Source?.PageId == pageId);

    private static Task<Guid> RecipeBySourceAsync(CulinaryBlogDbContext db, long pageId)
    {
        var marker = SeedTextCorrections.SourceMarker(pageId);
        return db.Recipes.IgnoreQueryFilters().Where(r => r.Instructions != null && r.Instructions.Contains(marker)).Select(r => r.Id).SingleAsync();
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
