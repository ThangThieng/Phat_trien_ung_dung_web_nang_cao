using System.Security.Cryptography;
using Bogus;
using CulinaryBlog.Application.Common.Caching;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; }

    public string AdminEmail { get; set; } = "admin@culinaryblog.local";

    /// <summary>Không có mật khẩu → không tạo tài khoản admin (tránh hardcode credential – NFR-SEC-007).</summary>
    public string? AdminPassword { get; set; }

    /// <summary>Mật khẩu chung cho 5 tác giả mẫu. Để trống → sinh ngẫu nhiên (tác giả mẫu không đăng nhập được).</summary>
    public string? AuthorPassword { get; set; }

    /// <summary>
    /// Bật thì lên lịch tải ảnh giấy phép mở về MinIO (CR-2026-07). Mặc định tắt: integration test và E2E không gọi mạng ngoài;
    /// khi đó công thức Wikibooks giữ ở Draft vì chưa có ảnh.
    /// </summary>
    public bool ImportImages { get; set; }

    /// <summary>Giây chờ trước mỗi lần tải ảnh. 10 giây: khi đo, Wikimedia trả 429 ở nhịp 5 giây và vẫn nhận khoảng 6 ảnh/phút khi đã bị giới hạn.</summary>
    public int ImageImportPaceSeconds { get; set; } = 10;

    /// <summary>Giây chờ khi nhận 429 không có header <c>Retry-After</c>.</summary>
    public int ImageImportRateLimitWaitSeconds { get; set; } = 60;
}

/// <summary>
/// Seed dữ liệu theo SRS §2.6.1 (CR-2026-03, CR-2026-07): roles Author/Admin (§2.3), 5 tác giả mẫu ("Tác giả mẫu 1…5"),
/// ≥ 20 danh mục và ≥ 100 công thức lấy từ <see cref="RecipeSeedCatalog"/>, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước.
/// Idempotent và tự bù: chỉ thêm phần còn thiếu; công thức mẫu cũ (chưa từng bị sửa, thiếu nguyên liệu/bước)
/// được thay bằng nội dung đúng trong catalog. Không động vào dữ liệu người dùng tạo hoặc đã chỉnh sửa.
/// Công thức Wikibooks tạo ở Draft; <see cref="SeedImageImportJob"/> chỉ xuất bản sau khi ảnh đã lưu.
/// </summary>
public sealed partial class DatabaseSeeder(
    CulinaryBlogDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedOptions> options,
    ICacheService cache,
    ISeedImageImportScheduler imageImports,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    private const int SeedAuthorCount = 5;

    /// <summary>Slug (cũ và mới) của công thức vừa được <see cref="ApplyTextCorrectionsAsync"/> sửa — xóa cache sau commit.</summary>
    private readonly List<(string Slug, RecipeStatus Status)> correctedSlugs = [];

    /// <summary>Trả về true khi có danh mục/công thức được thêm hoặc sửa – nơi gọi dùng để xóa Output Cache công thức.</summary>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return false;
        }

        await SeedRolesAsync().ConfigureAwait(false);
        await SeedAdminAsync().ConfigureAwait(false);
        var authors = await SeedAuthorsAsync().ConfigureAwait(false);

        var strategy = db.Database.CreateExecutionStrategy();
        var (categoriesAdded, recipesChanged, publishDates) = await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                var (categories, added) = await SeedCategoriesAsync(ct).ConfigureAwait(false);
                var (created, repaired, dates) = authors.Count == 0
                    ? (0, 0, new Dictionary<string, DateTime>(StringComparer.Ordinal))
                    : await SeedRecipesAsync(categories, authors, ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);
                LogSeeded(logger, categories.Count, authors.Count, created, repaired);
                return (added > 0, created + repaired > 0, dates);
            },
            cancellationToken).ConfigureAwait(false);

        // Danh sách danh mục được cache Redis 60 phút (FR-CAT-001) – bỏ bản cũ để thấy ngay danh mục vừa thêm.
        if (categoriesAdded)
        {
            await cache.RemoveAsync(CategoryCacheKeys.All, cancellationToken).ConfigureAwait(false);
        }

        // Bản dịch vừa sửa phải hiện ngay, không chờ recipe:{slug} (5′) và recipes:list (2′) hết hạn; công thức đổi tên còn làm đổi sitemap.
        foreach (var (slug, status) in correctedSlugs)
        {
            await cache.InvalidateAsync(RecipeCacheKeys.ForVisibilityChange(slug, status, status), cancellationToken).ConfigureAwait(false);
        }

        correctedSlugs.Clear();

        // Sau commit: job cần đọc được công thức. Lần khởi động sau tự bù phần ảnh còn thiếu.
        if (options.Value.ImportImages && authors.Count > 0)
        {
            await ScheduleImageImportsAsync(authors, publishDates, cancellationToken).ConfigureAwait(false);
        }

        return categoriesAdded || recipesChanged;
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { Roles.Author, Roles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
            }
        }
    }

    /// <summary>Admin được gán thủ công qua database seeding (SRS §2.3).</summary>
    private async Task SeedAdminAsync()
    {
        var seed = options.Value;
        if (await userManager.FindByEmailAsync(seed.AdminEmail).ConfigureAwait(false) is not null)
        {
            return;
        }

        // 19/11/2026: máy mới dựng từ .env.example (chưa chạy tools/dev-setup.sh) có role Admin mà không có tài khoản Admin, và
        // không có dòng log nào giải thích. Ghi rõ lý do để người dựng máy biết phải đặt biến nào.
        if (string.IsNullOrWhiteSpace(seed.AdminPassword))
        {
            LogAdminSkipped(logger, seed.AdminEmail);
            return;
        }

        var admin = ApplicationUser.Create("Quản trị viên", seed.AdminEmail, "admin", timeProvider.GetUtcNow().UtcDateTime);
        admin.EmailConfirmed = true;
        var result = await userManager.CreateAsync(admin, seed.AdminPassword).ConfigureAwait(false);
        if (result.Succeeded)
        {
            await userManager.AddToRolesAsync(admin, [Roles.Admin, Roles.Author]).ConfigureAwait(false);
            LogAdminCreated(logger, seed.AdminEmail);
        }
        else
        {
            LogAdminFailed(logger, seed.AdminEmail, string.Join(", ", result.Errors.Select(error => error.Code)));
        }
    }

    /// <summary>Thêm danh mục còn thiếu (so theo tên và slug, kể cả bản ghi đã xóa mềm để không vi phạm UNIQUE). Trả về danh mục đang hoạt động theo tên.</summary>
    private async Task<(Dictionary<string, Category> Active, int Added)> SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        var existing = await db.Categories.IgnoreQueryFilters().ToListAsync(cancellationToken).ConfigureAwait(false);
        var order = existing.Count == 0 ? 0 : existing.Max(c => c.OrderIndex) + 1;
        var added = 0;

        foreach (var seed in RecipeSeedCatalog.Categories)
        {
            var slug = SlugHelper.Generate(seed.Name);
            if (existing.Exists(c => c.Name == seed.Name || c.Slug == slug))
            {
                continue;
            }

            var category = Category.Create(seed.Name, slug, seed.Description, orderIndex: order++);
            existing.Add(category);
            db.Categories.Add(category);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (existing.Where(c => !c.IsDeleted).ToDictionary(c => c.Name, StringComparer.Ordinal), added);
    }

    private async Task<List<ApplicationUser>> SeedAuthorsAsync()
    {
        // Chỉ để nhận ra tên và giới thiệu mà bản cũ đã sinh (cùng seed 2026, cùng thứ tự gọi) — không dùng cho dữ liệu mới.
        var legacyFaker = new Faker("vi") { Random = new Randomizer(2026) };
        var authors = new List<ApplicationUser>();

        for (var i = 1; i <= SeedAuthorCount; i++)
        {
            var legacyName = legacyFaker.Name.FullName();
            var legacyBio = $"Đầu bếp tại gia, đam mê ẩm thực {legacyFaker.PickRandom("miền Bắc", "miền Trung", "miền Nam")}.";
            var email = $"author{i}@culinaryblog.local";
            var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user is null)
            {
                user = ApplicationUser.Create(RecipeSeedCatalog.SampleAuthorName(i), email, $"author{i}", timeProvider.GetUtcNow().UtcDateTime);
                user.EmailConfirmed = true;
                user.Bio = RecipeSeedCatalog.SampleAuthorBio;

                var password = string.IsNullOrWhiteSpace(options.Value.AuthorPassword)
                    ? $"Aa1!{Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))}"
                    : options.Value.AuthorPassword;

                var result = await userManager.CreateAsync(user, password).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    continue;
                }

                await userManager.AddToRoleAsync(user, Roles.Author).ConfigureAwait(false);
            }
            else if (user.DisplayName == legacyName && user.Bio == legacyBio)
            {
                // Database cũ: đổi sang tên mẫu. Ai đã tự sửa hồ sơ (FR-AUTH-007) thì giữ nguyên, không ghi đè.
                user.DisplayName = RecipeSeedCatalog.SampleAuthorName(i);
                user.Bio = RecipeSeedCatalog.SampleAuthorBio;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
            }

            authors.Add(user);
        }

        return authors;
    }

    private async Task<(int Created, int Repaired, Dictionary<string, DateTime> PublishDates)> SeedRecipesAsync(
        Dictionary<string, Category> categories,
        List<ApplicationUser> authors,
        CancellationToken cancellationToken)
    {
        var seedAuthorIds = authors.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);

        // Trước khi đối chiếu slug: công thức đổi tên ở đây sẽ mang slug mới, nên vòng tạo bên dưới không thêm bản trùng.
        var corrected = await ApplyTextCorrectionsAsync(seedAuthorIds, cancellationToken).ConfigureAwait(false);

        // Unique index chỉ áp dụng cho dòng chưa xóa (D-3): một slug có thể vừa có bản đã xóa mềm vừa có bản đang hoạt động.
        // Ưu tiên bản đang hoạt động thay vì để ToDictionary ném lỗi lúc khởi động.
        var existing = (await db.Recipes.IgnoreQueryFilters()
            .Select(r => new ExistingRecipe(r.Id, r.Slug, r.AuthorId, r.UpdatedAt, r.Ingredients.Count, r.Steps.Count, r.IsDeleted))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .GroupBy(r => r.Slug, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.IsDeleted).First(), StringComparer.Ordinal);

        // Bogus với seed cố định: mỗi món luôn rút cùng một chuỗi số ngẫu nhiên → dữ liệu ổn định giữa các lần chạy.
        var faker = new Faker("vi") { Random = new Randomizer(20260611) };
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var created = 0;
        var repaired = 0;
        var publishDates = new Dictionary<string, DateTime>(StringComparer.Ordinal);

        foreach (var seed in RecipeSeedCatalog.Recipes)
        {
            var author = faker.PickRandom(authors);
            var publish = faker.Random.Bool(0.85f);
            var publishedAt = now.AddDays(-faker.Random.Int(0, 180)).AddMinutes(-faker.Random.Int(0, 1440));

            if (!categories.TryGetValue(seed.Category, out var category))
            {
                continue;
            }

            var slug = SlugHelper.Generate(seed.Title);
            if (seed.Source is not null)
            {
                publishDates[slug] = publishedAt;
            }

            if (!existing.TryGetValue(slug, out var current))
            {
                var recipe = Recipe.Create(
                    seed.Title,
                    slug,
                    seed.Description,
                    category.Id,
                    author.Id,
                    seed.PrepTimeMinutes,
                    seed.CookTimeMinutes,
                    seed.Servings,
                    seed.Difficulty,
                    seed.Attribution ?? RecipeSeedCatalog.InstructionsSummary(seed));
                AddContent(recipe, seed);
                recipe.SetNutrition(seed.Nutrition);

                // Công thức do nhóm biên soạn: ~85% Published, còn lại Draft để kiểm thử phân quyền xem Draft.
                // Công thức Wikibooks giữ Draft; SeedImageImportJob xuất bản sau khi có ảnh (NFR-SEO-001).
                if (publish && seed.Source is null)
                {
                    recipe.Publish(publishedAt);
                }

                db.Recipes.Add(recipe);
                created++;
            }
            else if (NeedsRepair(current, seedAuthorIds))
            {
                await RepairAsync(current.Id, seed, cancellationToken).ConfigureAwait(false);
                repaired++;
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (created, repaired + corrected, publishDates);
    }

    /// <summary>
    /// Đưa <see cref="SeedTextCorrections"/> vào database đã seed từ trước. Cập nhật bằng ExecuteUpdate có điều kiện "giá trị hiện tại
    /// đúng bằng văn bản cũ", nên bản người dùng đã sửa không bị ghi đè. Trả về số công thức đã đổi; khóa cache của chúng được xóa
    /// sau khi transaction commit (<see cref="correctedSlugs"/>).
    /// </summary>
    private async Task<int> ApplyTextCorrectionsAsync(HashSet<string> seedAuthorIds, CancellationToken cancellationToken)
    {
        var changed = new HashSet<Guid>();
        foreach (var group in SeedTextCorrections.All.GroupBy(c => c.PageId))
        {
            var marker = SeedTextCorrections.SourceMarker(group.Key);
            var recipe = await db.Recipes.AsNoTracking()
                .Where(r => r.Instructions != null && r.Instructions.Contains(marker) && seedAuthorIds.Contains(r.AuthorId))
                .Select(r => new { r.Id, r.Slug, r.Status })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (recipe is null)
            {
                continue;
            }

            var rows = 0;
            foreach (var fix in group)
            {
                rows += await ApplyAsync(recipe.Id, fix, cancellationToken).ConfigureAwait(false);
            }

            if (rows > 0)
            {
                changed.Add(recipe.Id);
                correctedSlugs.Add((recipe.Slug, recipe.Status));
                var renamed = group.FirstOrDefault(c => c.Field == SeedTextField.Title);
                if (renamed is not null)
                {
                    correctedSlugs.Add((SlugHelper.Generate(renamed.New), recipe.Status));
                }
            }
        }

        return changed.Count;
    }

    private async Task<int> ApplyAsync(Guid recipeId, SeedTextCorrection fix, CancellationToken cancellationToken)
    {
        switch (fix.Field)
        {
            case SeedTextField.Title:
                var slug = SlugHelper.Generate(fix.New);
                if (await db.Recipes.AnyAsync(r => r.Slug == slug, cancellationToken).ConfigureAwait(false))
                {
                    return 0;
                }

                return await db.Recipes.IgnoreQueryFilters()
                    .Where(r => r.Id == recipeId && r.Title == fix.Old)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Title, fix.New).SetProperty(r => r.Slug, slug), cancellationToken)
                    .ConfigureAwait(false);
            case SeedTextField.Description:
                return await db.Recipes.IgnoreQueryFilters()
                    .Where(r => r.Id == recipeId && r.Description.Contains(fix.Old))
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Description, r => r.Description.Replace(fix.Old, fix.New)), cancellationToken)
                    .ConfigureAwait(false);
            case SeedTextField.IngredientName:
                return await db.Set<RecipeIngredient>().IgnoreQueryFilters()
                    .Where(i => i.RecipeId == recipeId && i.Name == fix.Old)
                    .ExecuteUpdateAsync(s => s.SetProperty(i => i.Name, fix.New), cancellationToken)
                    .ConfigureAwait(false);
            default:
                return await db.Set<RecipeStep>().IgnoreQueryFilters()
                    .Where(st => st.RecipeId == recipeId && st.Description.Contains(fix.Old))
                    .ExecuteUpdateAsync(s => s.SetProperty(st => st.Description, st => st.Description.Replace(fix.Old, fix.New)), cancellationToken)
                    .ConfigureAwait(false);
        }
    }

    /// <summary>Chỉ công thức của tác giả mẫu, chưa có ảnh (kể cả ảnh đã bị xóa mềm: người dùng đã chủ động bỏ thì không gắn lại).</summary>
    private async Task ScheduleImageImportsAsync(
        List<ApplicationUser> authors,
        Dictionary<string, DateTime> publishDates,
        CancellationToken cancellationToken)
    {
        var seedAuthorIds = authors.Select(a => a.Id).ToList();
        var slugs = RecipeSeedCatalog.Recipes
            .Where(r => r.Image is not null)
            .Select(r => SlugHelper.Generate(r.Title))
            .Concat(RecipeSeedCatalog.LegacyImages.Keys)
            .ToList();

        var pending = await db.Recipes.IgnoreQueryFilters().AsNoTracking()
            .Where(r => !r.IsDeleted && slugs.Contains(r.Slug) && seedAuthorIds.Contains(r.AuthorId) && !r.Images.Any())
            .OrderBy(r => r.Slug)
            .Select(r => new { r.Id, r.Slug, r.Status, r.PublishedAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var recipe in pending)
        {
            DateTime? publishAt = recipe.Status == RecipeStatus.Draft && recipe.PublishedAt is null && publishDates.TryGetValue(recipe.Slug, out var date)
                ? date
                : null;
            imageImports.Enqueue(recipe.Id, publishAt);
        }

        LogImagesScheduled(logger, pending.Count);
    }

    /// <summary>
    /// Chỉ sửa công thức mẫu do seeder tạo (tác giả mẫu), chưa từng được ai chỉnh sửa (UpdatedAt null)
    /// và chưa đạt ngưỡng nguyên liệu/bước – tức bản seed cũ 50 món với nội dung sinh ngẫu nhiên.
    /// </summary>
    private static bool NeedsRepair(ExistingRecipe recipe, HashSet<string> seedAuthorIds) =>
        seedAuthorIds.Contains(recipe.AuthorId)
        && recipe.UpdatedAt is null
        && (recipe.IngredientCount < RecipeSeedCatalog.MinIngredientsPerRecipe || recipe.StepCount < RecipeSeedCatalog.MinStepsPerRecipe);

    private async Task RepairAsync(Guid recipeId, SeedRecipe seed, CancellationToken cancellationToken)
    {
        // Xóa nội dung sinh ngẫu nhiên cũ rồi nạp lại từ catalog; StepNumber đánh lại từ 1 (SRS FR-RCP-010).
        await db.Set<RecipeIngredient>().IgnoreQueryFilters().Where(i => i.RecipeId == recipeId)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.Set<RecipeStep>().IgnoreQueryFilters().Where(s => s.RecipeId == recipeId)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        var recipe = await db.Recipes.IgnoreQueryFilters().SingleAsync(r => r.Id == recipeId, cancellationToken).ConfigureAwait(false);
        var entry = db.Entry(recipe);
        entry.Property(r => r.Description).CurrentValue = seed.Description;
        entry.Property(r => r.Instructions).CurrentValue = RecipeSeedCatalog.InstructionsSummary(seed);
        entry.Property(r => r.PrepTimeMinutes).CurrentValue = seed.PrepTimeMinutes;
        entry.Property(r => r.CookTimeMinutes).CurrentValue = seed.CookTimeMinutes;
        entry.Property(r => r.Servings).CurrentValue = seed.Servings;
        entry.Property(r => r.Difficulty).CurrentValue = seed.Difficulty;
        recipe.SetNutrition(seed.Nutrition);

        AddContent(recipe, seed);
    }

    private void AddContent(Recipe recipe, SeedRecipe seed)
    {
        foreach (var ingredient in seed.Ingredients)
        {
            db.Add(recipe.AddIngredient(new IngredientDetails(ingredient.Name, ingredient.Quantity, ingredient.QuantityText, ingredient.Unit, ingredient.Notes)));
        }

        foreach (var step in seed.Steps)
        {
            db.Add(recipe.AddStep(step.Title, step.Description, step.TimerMinutes));
        }
    }

    private sealed record ExistingRecipe(Guid Id, string Slug, string AuthorId, DateTime? UpdatedAt, int IngredientCount, int StepCount, bool IsDeleted);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Admin account {AdminEmail} was NOT created: Seed:AdminPassword is empty. Set SEED_ADMIN_PASSWORD in .env (or run tools/dev-setup.sh) and restart the api container")]
    private static partial void LogAdminSkipped(ILogger logger, string adminEmail);

    [LoggerMessage(Level = LogLevel.Information, Message = "Admin account {AdminEmail} created")]
    private static partial void LogAdminCreated(ILogger logger, string adminEmail);

    [LoggerMessage(Level = LogLevel.Error, Message = "Admin account {AdminEmail} could not be created ({Errors}). SEED_ADMIN_PASSWORD needs at least 8 characters with upper case, lower case, a digit and a symbol (NFR-SEC-001)")]
    private static partial void LogAdminFailed(ILogger logger, string adminEmail, string errors);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database seeded: {Categories} categories, {Authors} authors, {Created} recipes created, {Repaired} recipes repaired")]
    private static partial void LogSeeded(ILogger logger, int categories, int authors, int created, int repaired);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed images scheduled: {Count} recipes")]
    private static partial void LogImagesScheduled(ILogger logger, int count);
}
