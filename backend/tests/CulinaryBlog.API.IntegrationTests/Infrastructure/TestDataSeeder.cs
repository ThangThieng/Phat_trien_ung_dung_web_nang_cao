using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// Bộ dữ liệu cố định cho integration test — CỐ TÌNH nhỏ và tất định, không dùng
/// <c>DatabaseSeeder</c> (100 công thức sinh bằng Bogus) của ứng dụng: test phải khẳng định được
/// "đúng 3 công thức Published" thay vì "ít nhất 1", và mỗi lần Respawn reset phải nạp lại trong vài
/// mili giây thay vì vài giây. Độ đầy đủ của bộ seed thật đã có <c>RecipeSeedCatalogTests</c> lo.
/// </summary>
public static class TestDataSeeder
{
    /// <summary>Secret của <c>NginxGateDashboardFilter</c> trong môi trường test (không phải secret thật).</summary>
    public const string HangfireGateSecret = "integration-test-hangfire-gate-secret";

    /// <summary>Mật khẩu hợp lệ theo NFR-SEC-001 (hoa + thường + số + ký tự đặc biệt, ≥ 8).</summary>
    public const string ValidPassword = "Culinary#2026";

    public const string AdminUserId = "11111111-1111-1111-1111-111111111111";
    public const string AuthorUserId = "22222222-2222-2222-2222-222222222222";

    /// <summary>Tác giả thứ hai – dùng để kiểm chứng "người khác không xóa được file của tôi" (FR-FILE-002 A1).</summary>
    public const string OtherAuthorUserId = "33333333-3333-3333-3333-333333333333";

    public const string AdminEmail = "it-admin@culinaryblog.test";
    public const string AuthorEmail = "it-author@culinaryblog.test";

    public const string PublishedRecipeSlug = "pho-bo-ha-noi";
    public const string DraftRecipeSlug = "banh-mi-thit-nuong-nhap";

    /// <summary>Số công thức Published mà <c>GET /recipes</c> phải trả về cho Guest.</summary>
    public const int PublishedRecipeCount = 3;

    public static readonly Guid MonChinhCategoryId = new("44444444-4444-4444-4444-444444444444");
    public static readonly Guid TrangMiengCategoryId = new("55555555-5555-5555-5555-555555555555");

    public static async Task SeedAsync(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var db = services.GetRequiredService<CulinaryBlogDbContext>();
        db.ChangeTracker.Clear();

        await SeedRolesAsync(services).ConfigureAwait(false);
        await SeedUsersAsync(services).ConfigureAwait(false);
        await SeedContentAsync(db).ConfigureAwait(false);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Author" })
        {
            if (!await roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
            }
        }
    }

    private static async Task SeedUsersAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureUserAsync(userManager, AdminUserId, "IT Admin", AdminEmail, "it_admin", "Admin").ConfigureAwait(false);
        await EnsureUserAsync(userManager, AuthorUserId, "IT Author", AuthorEmail, "it_author", "Author").ConfigureAwait(false);
        await EnsureUserAsync(userManager, OtherAuthorUserId, "IT Author 2", "it-author2@culinaryblog.test", "it_author2", "Author").ConfigureAwait(false);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string id,
        string displayName,
        string email,
        string userName,
        string role)
    {
        if (await userManager.FindByIdAsync(id).ConfigureAwait(false) is not null)
        {
            return;
        }

        var user = ApplicationUser.Create(displayName, email, userName, DateTime.UtcNow);
        user.Id = id;
        user.EmailConfirmed = true;

        var created = await userManager.CreateAsync(user, ValidPassword).ConfigureAwait(false);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException($"Không tạo được user test {email}: {string.Join("; ", created.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
    }

    private static async Task SeedContentAsync(CulinaryBlogDbContext db)
    {
        if (await db.Categories.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        var monChinh = Category.Create("Món chính", "mon-chinh", "Các món ăn chính trong bữa cơm.", orderIndex: 1);
        SetId(monChinh, MonChinhCategoryId);
        var trangMieng = Category.Create("Tráng miệng", "trang-mieng", "Món ngọt sau bữa ăn.", orderIndex: 2);
        SetId(trangMieng, TrangMiengCategoryId);

        db.Categories.AddRange(monChinh, trangMieng);

        var now = DateTime.UtcNow;

        db.Recipes.Add(BuildRecipe("Phở bò Hà Nội", PublishedRecipeSlug, MonChinhCategoryId, RecipeDifficulty.Hard, 40, 180, publishedAt: now));
        db.Recipes.Add(BuildRecipe("Bún chả Hà Nội", "bun-cha-ha-noi", MonChinhCategoryId, RecipeDifficulty.Medium, 30, 45, publishedAt: now));
        db.Recipes.Add(BuildRecipe("Chè đậu xanh", "che-dau-xanh", TrangMiengCategoryId, RecipeDifficulty.Easy, 15, 30, publishedAt: now));

        // Công thức Draft: dùng cho FR-RCP-002 A2 (Guest → 403) và cho test tái hiện MT-34.
        db.Recipes.Add(BuildRecipe("Bánh mì thịt nướng (nháp)", DraftRecipeSlug, MonChinhCategoryId, RecipeDifficulty.Easy, 20, 20, publishedAt: null));

        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    private static Recipe BuildRecipe(
        string title,
        string slug,
        Guid categoryId,
        RecipeDifficulty difficulty,
        int prepMinutes,
        int cookMinutes,
        DateTime? publishedAt)
    {
        var recipe = Recipe.Create(
            title,
            slug,
            $"Mô tả cho {title} – dữ liệu integration test.",
            categoryId,
            AuthorUserId,
            prepMinutes,
            cookMinutes,
            servings: 4,
            difficulty);

        recipe.AddIngredient("Nguyên liệu chính", 500, "g");
        recipe.AddStep("Sơ chế", "Rửa sạch và để ráo nguyên liệu.");

        if (publishedAt.HasValue)
        {
            recipe.Publish(publishedAt.Value);
        }

        return recipe;
    }

    /// <summary>Id của BaseEntity là private set (Domain không mở cửa cho test) – gán qua EF backing field.</summary>
    private static void SetId(BaseEntity entity, Guid id) =>
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(entity, [id]);
}
