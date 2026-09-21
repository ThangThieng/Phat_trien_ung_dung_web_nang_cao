using System.Security.Cryptography;
using Bogus;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
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
}

/// <summary>
/// Seed dữ liệu theo SRS §2.6.1 (CR-2026-03): roles Author/Admin (§2.3), 5 tác giả mẫu (Bogus),
/// ≥ 20 danh mục và ≥ 100 công thức lấy từ <see cref="RecipeSeedCatalog"/>, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước.
/// Idempotent và tự bù: chỉ thêm phần còn thiếu; công thức mẫu cũ (chưa từng bị sửa, thiếu nguyên liệu/bước)
/// được thay bằng nội dung đúng trong catalog. Không động vào dữ liệu người dùng tạo hoặc đã chỉnh sửa.
/// </summary>
public sealed partial class DatabaseSeeder(
    CulinaryBlogDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedOptions> options,
    ICacheService cache,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    private const int SeedAuthorCount = 5;

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
        var (categoriesAdded, recipesChanged) = await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                var (categories, added) = await SeedCategoriesAsync(ct).ConfigureAwait(false);
                var (created, repaired) = authors.Count == 0
                    ? (0, 0)
                    : await SeedRecipesAsync(categories, authors, ct).ConfigureAwait(false);

                await transaction.CommitAsync(ct).ConfigureAwait(false);
                LogSeeded(logger, categories.Count, authors.Count, created, repaired);
                return (added > 0, created + repaired > 0);
            },
            cancellationToken).ConfigureAwait(false);

        // Danh sách danh mục được cache Redis 60 phút (FR-CAT-001) – bỏ bản cũ để thấy ngay danh mục vừa thêm.
        if (categoriesAdded)
        {
            await cache.RemoveAsync(CategoryCacheKeys.All, cancellationToken).ConfigureAwait(false);
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
        if (string.IsNullOrWhiteSpace(seed.AdminPassword) || await userManager.FindByEmailAsync(seed.AdminEmail).ConfigureAwait(false) is not null)
        {
            return;
        }

        var admin = ApplicationUser.Create("Quản trị viên", seed.AdminEmail, "admin", timeProvider.GetUtcNow().UtcDateTime);
        admin.EmailConfirmed = true;
        var result = await userManager.CreateAsync(admin, seed.AdminPassword).ConfigureAwait(false);
        if (result.Succeeded)
        {
            await userManager.AddToRolesAsync(admin, [Roles.Admin, Roles.Author]).ConfigureAwait(false);
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
        var faker = new Faker("vi") { Random = new Randomizer(2026) };
        var authors = new List<ApplicationUser>();

        for (var i = 1; i <= SeedAuthorCount; i++)
        {
            var fullName = faker.Name.FullName();
            var region = faker.PickRandom("miền Bắc", "miền Trung", "miền Nam");
            var email = $"author{i}@culinaryblog.local";
            var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user is null)
            {
                user = ApplicationUser.Create(fullName, email, $"author{i}", timeProvider.GetUtcNow().UtcDateTime);
                user.EmailConfirmed = true;
                user.Bio = $"Đầu bếp tại gia, đam mê ẩm thực {region}.";

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

            authors.Add(user);
        }

        return authors;
    }

    private async Task<(int Created, int Repaired)> SeedRecipesAsync(
        Dictionary<string, Category> categories,
        List<ApplicationUser> authors,
        CancellationToken cancellationToken)
    {
        var seedAuthorIds = authors.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        var existing = await db.Recipes.IgnoreQueryFilters()
            .Select(r => new ExistingRecipe(r.Id, r.Slug, r.AuthorId, r.UpdatedAt, r.Ingredients.Count, r.Steps.Count))
            .ToDictionaryAsync(r => r.Slug, StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);

        // Bogus với seed cố định: mỗi món luôn rút cùng một chuỗi số ngẫu nhiên → dữ liệu ổn định giữa các lần chạy.
        var faker = new Faker("vi") { Random = new Randomizer(20260611) };
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var created = 0;
        var repaired = 0;

        foreach (var seed in RecipeSeedCatalog.Recipes)
        {
            var author = faker.PickRandom(authors);
            var publish = faker.Random.Bool(0.85f);
            var publishedAt = now.AddDays(-faker.Random.Int(0, 180)).AddMinutes(-faker.Random.Int(0, 1440));
            var nutrition = NutritionFor(seed.Category, faker);

            if (!categories.TryGetValue(seed.Category, out var category))
            {
                continue;
            }

            var slug = SlugHelper.Generate(seed.Title);
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
                    RecipeSeedCatalog.InstructionsSummary(seed));
                AddContent(recipe, seed);
                recipe.SetNutrition(nutrition);

                // ~85% Published, còn lại Draft để kiểm thử phân quyền xem Draft
                if (publish)
                {
                    recipe.Publish(publishedAt);
                }

                db.Recipes.Add(recipe);
                created++;
            }
            else if (NeedsRepair(current, seedAuthorIds))
            {
                await RepairAsync(current.Id, seed, nutrition, cancellationToken).ConfigureAwait(false);
                repaired++;
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (created, repaired);
    }

    /// <summary>
    /// Chỉ sửa công thức mẫu do seeder tạo (tác giả mẫu), chưa từng được ai chỉnh sửa (UpdatedAt null)
    /// và chưa đạt ngưỡng nguyên liệu/bước – tức bản seed cũ 50 món với nội dung sinh ngẫu nhiên.
    /// </summary>
    private static bool NeedsRepair(ExistingRecipe recipe, HashSet<string> seedAuthorIds) =>
        seedAuthorIds.Contains(recipe.AuthorId)
        && recipe.UpdatedAt is null
        && (recipe.IngredientCount < RecipeSeedCatalog.MinIngredientsPerRecipe || recipe.StepCount < RecipeSeedCatalog.MinStepsPerRecipe);

    private async Task RepairAsync(Guid recipeId, SeedRecipe seed, RecipeNutrition nutrition, CancellationToken cancellationToken)
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
        recipe.SetNutrition(nutrition);

        AddContent(recipe, seed);
    }

    private void AddContent(Recipe recipe, SeedRecipe seed)
    {
        foreach (var ingredient in seed.Ingredients)
        {
            db.Add(recipe.AddIngredient(ingredient.Name, ingredient.Quantity, ingredient.Unit, ingredient.Notes));
        }

        foreach (var step in seed.Steps)
        {
            db.Add(recipe.AddStep(step.Title, step.Description, step.TimerMinutes));
        }
    }

    /// <summary>Dinh dưỡng ước lượng cho 1 khẩu phần, theo khoảng hợp lý của từng nhóm món (SRS §7.2.1).</summary>
    private static RecipeNutrition NutritionFor(string category, Faker faker)
    {
        var (calories, protein, carbs, fat, fiber, sodium) = category switch
        {
            "Tráng miệng & Đồ uống" => ((120, 450), (2, 10), (20, 70), (3, 20), (0, 5), (20, 200)),
            "Món chay" => ((150, 450), (8, 25), (15, 60), (5, 20), (3, 12), (300, 900)),
            "Gỏi & Salad" => ((150, 400), (10, 30), (10, 35), (5, 20), (3, 10), (400, 1000)),
            _ => ((250, 750), (15, 45), (10, 80), (8, 35), (1, 8), (500, 1500)),
        };

        return new RecipeNutrition
        {
            Calories = Pick(faker, calories),
            Protein = Pick(faker, protein),
            Carbohydrates = Pick(faker, carbs),
            Fat = Pick(faker, fat),
            Fiber = Pick(faker, fiber),
            Sodium = Pick(faker, sodium),
        };

        static decimal Pick(Faker faker, (int Min, int Max) range) =>
            Math.Round(faker.Random.Decimal(range.Min, range.Max), 1);
    }

    private sealed record ExistingRecipe(Guid Id, string Slug, string AuthorId, DateTime? UpdatedAt, int IngredientCount, int StepCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database seeded: {Categories} categories, {Authors} authors, {Created} recipes created, {Repaired} recipes repaired")]
    private static partial void LogSeeded(ILogger logger, int categories, int authors, int created, int repaired);
}
