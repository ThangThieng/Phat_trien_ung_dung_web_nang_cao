using System.Security.Cryptography;
using Bogus;
using CulinaryBlog.Application.Common.Interfaces;
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
/// Seed dữ liệu theo SRS §2.6.1: 50 recipe mẫu, 5 tác giả mẫu (Bogus) + roles Author/Admin (§2.3) + danh mục.
/// Idempotent: chỉ seed phần chưa có.
/// </summary>
public sealed partial class DatabaseSeeder(
    CulinaryBlogDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedOptions> options,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    private static readonly (string Name, string Description)[] CategorySeeds =
    [
        ("Món khai vị", "Món ăn nhẹ mở đầu bữa ăn: gỏi, chả giò, nộm..."),
        ("Món chính", "Các món ăn chính cho bữa cơm gia đình."),
        ("Canh & Súp", "Canh, súp thanh mát và bổ dưỡng."),
        ("Món chay", "Món ăn thuần chay, tốt cho sức khỏe."),
        ("Món nướng", "Thịt, hải sản và rau củ nướng thơm lừng."),
        ("Bún & Phở", "Các món nước truyền thống của Việt Nam."),
        ("Bánh", "Bánh mặn, bánh ngọt, bánh truyền thống."),
        ("Tráng miệng & Đồ uống", "Chè, kem, sinh tố và các loại nước giải khát."),
    ];

    private static readonly (string Title, string Category)[] DishSeeds =
    [
        ("Phở bò Hà Nội", "Bún & Phở"), ("Phở gà", "Bún & Phở"), ("Bún chả Hà Nội", "Bún & Phở"), ("Bún bò Huế", "Bún & Phở"),
        ("Bún riêu cua", "Bún & Phở"), ("Hủ tiếu Nam Vang", "Bún & Phở"), ("Mì Quảng", "Bún & Phở"), ("Cao lầu Hội An", "Bún & Phở"),
        ("Gỏi cuốn tôm thịt", "Món khai vị"), ("Chả giò rế", "Món khai vị"), ("Nộm đu đủ bò khô", "Món khai vị"), ("Gỏi ngó sen tôm thịt", "Món khai vị"),
        ("Bánh khọt Vũng Tàu", "Món khai vị"), ("Nem chua rán", "Món khai vị"),
        ("Cá kho tộ", "Món chính"), ("Thịt kho trứng", "Món chính"), ("Gà kho gừng", "Món chính"), ("Sườn xào chua ngọt", "Món chính"),
        ("Bò lúc lắc", "Món chính"), ("Cơm tấm sườn bì chả", "Món chính"), ("Tôm rim mặn ngọt", "Món chính"), ("Mực xào sa tế", "Món chính"),
        ("Canh chua cá lóc", "Canh & Súp"), ("Canh bí đỏ nấu tôm", "Canh & Súp"), ("Súp cua trứng bắc thảo", "Canh & Súp"), ("Canh rau ngót thịt băm", "Canh & Súp"),
        ("Lẩu thái hải sản", "Canh & Súp"), ("Súp gà ngô non", "Canh & Súp"),
        ("Đậu hũ sốt cà chua", "Món chay"), ("Nấm kho tiêu", "Món chay"), ("Rau củ xào thập cẩm", "Món chay"), ("Canh nấm chay", "Món chay"),
        ("Cà tím nướng mỡ hành", "Món chay"), ("Bún xào chay", "Món chay"),
        ("Thịt heo nướng sả", "Món nướng"), ("Gà nướng mật ong", "Món nướng"), ("Bò nướng lá lốt", "Món nướng"), ("Cá nướng giấy bạc", "Món nướng"),
        ("Sườn nướng BBQ", "Món nướng"), ("Tôm nướng muối ớt", "Món nướng"),
        ("Bánh xèo miền Tây", "Bánh"), ("Bánh cuốn nóng", "Bánh"), ("Bánh bèo chén", "Bánh"), ("Bánh flan caramel", "Bánh"),
        ("Bánh mì thịt nguội", "Bánh"), ("Bánh chuối nướng", "Bánh"),
        ("Chè ba màu", "Tráng miệng & Đồ uống"), ("Chè bưởi", "Tráng miệng & Đồ uống"), ("Sinh tố bơ", "Tráng miệng & Đồ uống"), ("Cà phê trứng", "Tráng miệng & Đồ uống"),
    ];

    private static readonly string[] IngredientNames =
    [
        "Thịt bò thăn", "Thịt heo ba chỉ", "Đùi gà", "Tôm sú", "Mực ống", "Cá lóc", "Trứng gà", "Đậu hũ non", "Nấm rơm", "Hành tím",
        "Tỏi", "Gừng", "Sả", "Ớt", "Hành lá", "Rau mùi", "Nước mắm", "Đường", "Muối", "Tiêu", "Dầu ăn", "Nước cốt dừa", "Cà chua",
        "Bánh phở", "Bún tươi", "Gạo tẻ", "Bột gạo", "Đậu xanh", "Chanh", "Me chua",
    ];

    private static readonly string[] Units = ["gram", "ml", "thìa canh", "thìa cà phê", "quả", "củ", "nhánh", "bó"];

    private static readonly string[] StepTitles =
    [
        "Sơ chế nguyên liệu", "Ướp gia vị", "Chuẩn bị nước dùng", "Chế biến chính", "Nêm nếm lại", "Trình bày và thưởng thức",
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        await SeedRolesAsync().ConfigureAwait(false);
        await SeedAdminAsync().ConfigureAwait(false);
        var categories = await SeedCategoriesAsync(cancellationToken).ConfigureAwait(false);

        if (await db.Recipes.IgnoreQueryFilters().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var authors = await SeedAuthorsAsync().ConfigureAwait(false);
        var count = await SeedRecipesAsync(categories, authors, cancellationToken).ConfigureAwait(false);
        LogSeeded(logger, categories.Count, authors.Count, count);
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

    private async Task<List<Category>> SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        var existing = await db.Categories.ToListAsync(cancellationToken).ConfigureAwait(false);
        var order = existing.Count;

        foreach (var (name, description) in CategorySeeds.Where(s => existing.TrueForAll(c => c.Name != s.Name)))
        {
            var category = Category.Create(name, SlugHelper.Generate(name), description, orderIndex: order++);
            existing.Add(category);
            db.Categories.Add(category);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing;
    }

    private async Task<List<ApplicationUser>> SeedAuthorsAsync()
    {
        var faker = new Faker("vi") { Random = new Randomizer(2026) };
        var authors = new List<ApplicationUser>();

        for (var i = 1; i <= 5; i++)
        {
            var email = $"author{i}@culinaryblog.local";
            var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user is null)
            {
                user = ApplicationUser.Create(faker.Name.FullName(), email, $"author{i}", timeProvider.GetUtcNow().UtcDateTime);
                user.EmailConfirmed = true;
                user.Bio = $"Đầu bếp tại gia, đam mê ẩm thực {faker.PickRandom("miền Bắc", "miền Trung", "miền Nam")}.";

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

    private async Task<int> SeedRecipesAsync(List<Category> categories, List<ApplicationUser> authors, CancellationToken cancellationToken)
    {
        if (authors.Count == 0)
        {
            return 0;
        }

        var faker = new Faker("vi") { Random = new Randomizer(20260611) };
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var (title, categoryName) in DishSeeds)
        {
            var category = categories.First(c => c.Name == categoryName);
            var recipe = Recipe.Create(
                title,
                SlugHelper.Generate(title),
                $"{title} – công thức chuẩn vị, dễ làm tại nhà. {faker.Lorem.Sentence(12)}",
                category.Id,
                faker.PickRandom(authors).Id,
                prepTimeMinutes: faker.Random.Int(10, 45),
                cookTimeMinutes: faker.Random.Int(0, 120),
                servings: faker.Random.Int(1, 8),
                difficulty: faker.PickRandom<RecipeDifficulty>(),
                instructions: "Xem chi tiết từng bước bên dưới.");

            foreach (var name in faker.PickRandom(IngredientNames, faker.Random.Int(4, 9)))
            {
                recipe.AddIngredient(name, faker.Random.Bool(0.85f) ? faker.Random.Int(1, 50) * 10 : null, faker.PickRandom(Units), faker.Random.Bool(0.3f) ? "thái lát mỏng" : null);
            }

            foreach (var stepTitle in StepTitles.Take(faker.Random.Int(3, StepTitles.Length)))
            {
                recipe.AddStep(stepTitle, faker.Lorem.Paragraph(2), faker.Random.Bool(0.6f) ? faker.Random.Int(2, 30) : null);
            }

            recipe.SetNutrition(new RecipeNutrition
            {
                Calories = faker.Random.Decimal(120, 850),
                Protein = faker.Random.Decimal(2, 60),
                Carbohydrates = faker.Random.Decimal(5, 120),
                Fat = faker.Random.Decimal(1, 45),
                Fiber = faker.Random.Decimal(0, 15),
                Sodium = faker.Random.Decimal(50, 1800),
            });

            // ~85% Published, còn lại Draft để kiểm thử phân quyền xem Draft
            if (faker.Random.Bool(0.85f))
            {
                recipe.Publish(now.AddDays(-faker.Random.Int(0, 180)).AddMinutes(-faker.Random.Int(0, 1440)));
            }

            db.Recipes.Add(recipe);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return DishSeeds.Length;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Database seeded: {Categories} categories, {Authors} authors, {Recipes} recipes")]
    private static partial void LogSeeded(ILogger logger, int categories, int authors, int recipes);
}
