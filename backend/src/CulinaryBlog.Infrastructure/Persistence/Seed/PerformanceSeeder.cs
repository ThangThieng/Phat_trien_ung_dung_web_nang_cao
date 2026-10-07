using Bogus;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public sealed class PerformanceSeedOptions
{
    public const string SectionName = "PerformanceSeed";

    /// <summary>Mặc định TẮT: bộ dữ liệu lớn chỉ dùng để đo <c>EXPLAIN ANALYZE</c> (NFR-PERF-004), không bao giờ bật ở production.</summary>
    public bool Enabled { get; set; }

    /// <summary>Số công thức tạo thêm; SRS yêu cầu đo trên ≥ 10.000 bản ghi.</summary>
    public int RecipeCount { get; set; } = 10_000;
}

/// <summary>
/// NFR-PERF-004 (Buổi 5 — Dev 3): sinh ≥ 10.000 công thức để PostgreSQL có đủ thống kê chọn Index Scan. Với 100 bản ghi seed
/// (CR-2026-03) planner luôn chọn Seq Scan vì quét cả bảng rẻ hơn dùng index, nên số đo trên dữ liệu nhỏ không chứng minh được gì.
/// Chạy SAU <see cref="DatabaseSeeder"/> (cần danh mục và tác giả mẫu). Idempotent: nhận diện bản ghi của mình qua slug
/// <c>perf-…</c> và chỉ bổ sung phần còn thiếu. Mỗi công thức chỉ có 1 bước + 1 nguyên liệu (tối thiểu để Publish được) vì mục
/// tiêu là bảng <c>Recipes</c>, không phải bảng con.
/// </summary>
public sealed partial class PerformanceSeeder(
    CulinaryBlogDbContext db,
    IOptions<PerformanceSeedOptions> options,
    TimeProvider timeProvider,
    ILogger<PerformanceSeeder> logger)
{
    private const string SlugPrefix = "perf-";
    private const int BatchSize = 1000;

    private static readonly string[] Dishes =
        ["Phở", "Bún", "Cơm", "Gỏi", "Canh", "Chè", "Bánh", "Lẩu", "Nem", "Xôi", "Cháo", "Mì"];

    private static readonly string[] Styles =
        ["bò", "gà", "heo", "tôm", "cá", "chay", "nấm", "rau củ", "hải sản", "trứng"];

    public async Task<int> SeedAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.Enabled || settings.RecipeCount <= 0)
        {
            return 0;
        }

        var categoryIds = (await db.Categories.Select(c => c.Id).ToListAsync(cancellationToken).ConfigureAwait(false)).ToArray();
        var authorIds = await db.Users
            .Where(u => u.Email != null && u.Email.StartsWith("author"))
            .Select(u => u.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (categoryIds.Length == 0 || authorIds.Length == 0)
        {
            LogMissingBaseData(logger);
            return 0;
        }

        var existing = await db.Recipes.IgnoreQueryFilters()
            .CountAsync(r => r.Slug.StartsWith(SlugPrefix), cancellationToken)
            .ConfigureAwait(false);
        var missing = settings.RecipeCount - existing;
        if (missing <= 0)
        {
            return 0;
        }

        // Seed cố định: cùng một bộ dữ liệu ở mọi lần chạy để số đo so sánh được trước/sau khi thêm index.
        var random = new Randomizer(20261007);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var difficulties = Enum.GetValues<RecipeDifficulty>();
        var created = 0;

        while (created < missing)
        {
            db.ChangeTracker.Clear();
            var batch = Math.Min(BatchSize, missing - created);

            for (var i = 0; i < batch; i++)
            {
                var number = existing + created + i + 1;
                var recipe = Recipe.Create(
                    $"{random.ArrayElement(Dishes)} {random.ArrayElement(Styles)} {number}",
                    $"{SlugPrefix}{number:D6}",
                    $"Công thức mẫu hiệu năng số {number}.",
                    random.ArrayElement(categoryIds),
                    random.ArrayElement(authorIds),
                    random.Int(5, 60),
                    random.Int(0, 180),
                    random.Int(1, 8),
                    random.ArrayElement(difficulties));
                recipe.AddStep("Chế biến", "Thực hiện theo hướng dẫn.");
                recipe.AddIngredient("Nguyên liệu chính", 1, "phần");

                // ~85% Published như dữ liệu seed thật; còn lại Draft để truy vấn công khai phải loại bớt.
                if (random.Bool(0.85f))
                {
                    recipe.Publish(now.AddDays(-random.Int(0, 365)).AddMinutes(-random.Int(0, 1440)));
                }

                db.Recipes.Add(recipe);
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            created += batch;
        }

        LogSeeded(logger, created);
        return created;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "PerformanceSeeder: đã tạo {Created} công thức mẫu hiệu năng")]
    private static partial void LogSeeded(ILogger logger, int created);

    [LoggerMessage(Level = LogLevel.Warning, Message = "PerformanceSeeder: chưa có danh mục/tác giả mẫu — bật Seed:Enabled=true trước")]
    private static partial void LogMissingBaseData(ILogger logger);
}
