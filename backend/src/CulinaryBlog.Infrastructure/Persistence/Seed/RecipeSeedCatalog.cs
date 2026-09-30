using System.Globalization;
using System.Reflection;
using System.Text.Json;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public sealed record SeedCategory(string Name, string Description);

public sealed record SeedIngredient(string Name, decimal? Quantity, string? Unit, string? Notes);

public sealed record SeedStep(string Title, string Description, int? TimerMinutes);

public sealed record SeedRecipe(
    string Title,
    string Category,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    IReadOnlyList<SeedIngredient> Ingredients,
    IReadOnlyList<SeedStep> Steps);

/// <summary>
/// Bộ dữ liệu mẫu theo SRS §2.6.1 (CR-2026-03): ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước.
/// Nội dung món ăn (nguyên liệu, định lượng, các bước) là công thức thật, viết tay trong <c>Seed/Data/*.json</c>;
/// Bogus chỉ sinh phần ngẫu nhiên hợp lý (tác giả, trạng thái, ngày xuất bản, dinh dưỡng) trong <see cref="DatabaseSeeder"/>.
/// </summary>
public static class RecipeSeedCatalog
{
    public const int MinCategories = 20;
    public const int MinRecipes = 100;
    public const int MinIngredientsPerRecipe = 10;
    public const int MinStepsPerRecipe = 5;

    private const string ResourcePrefix = "SeedData/";
    private static readonly Lazy<(IReadOnlyList<SeedCategory> Categories, IReadOnlyList<SeedRecipe> Recipes)> Data = new(Load);

    public static IReadOnlyList<SeedCategory> Categories => Data.Value.Categories;

    public static IReadOnlyList<SeedRecipe> Recipes => Data.Value.Recipes;

    /// <summary>Tóm tắt dùng làm cột Instructions (SRS §7.2) – nội dung chi tiết nằm trong RecipeSteps.</summary>
    public static string InstructionsSummary(SeedRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"Thực hiện theo {recipe.Steps.Count} bước bên dưới: {string.Join(" → ", recipe.Steps.Select(s => s.Title))}.");
    }

    private static (IReadOnlyList<SeedCategory> Categories, IReadOnlyList<SeedRecipe> Recipes) Load()
    {
        var assembly = typeof(RecipeSeedCatalog).Assembly;

        var categories = new List<SeedCategory>();
        using (var document = Read(assembly, $"{ResourcePrefix}categories.json"))
        {
            foreach (var item in document.RootElement.EnumerateArray())
            {
                categories.Add(new SeedCategory(item.GetProperty("name").GetString()!, item.GetProperty("description").GetString()!));
            }
        }

        var recipes = new List<SeedRecipe>();
        var recipeResources = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith($"{ResourcePrefix}recipes-", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);

        foreach (var resource in recipeResources)
        {
            using var document = Read(assembly, resource);
            recipes.AddRange(document.RootElement.EnumerateArray().Select(ParseRecipe));
        }

        return (categories, recipes);
    }

    private static JsonDocument Read(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Không tìm thấy dữ liệu seed '{resourceName}'.");
        return JsonDocument.Parse(stream);
    }

    private static SeedRecipe ParseRecipe(JsonElement item)
    {
        // Nguyên liệu: [tên, định lượng | null, đơn vị | null, ghi chú?]; bước: [tiêu đề, mô tả, số phút | null]
        var ingredients = item.GetProperty("ingredients").EnumerateArray()
            .Select(i => new SeedIngredient(
                i[0].GetString()!,
                i[1].ValueKind == JsonValueKind.Null ? null : i[1].GetDecimal(),
                i[2].GetString(),
                i.GetArrayLength() > 3 ? i[3].GetString() : null))
            .ToList();

        var steps = item.GetProperty("steps").EnumerateArray()
            .Select(s => new SeedStep(
                s[0].GetString()!,
                s[1].GetString()!,
                s[2].ValueKind == JsonValueKind.Null ? null : s[2].GetInt32()))
            .ToList();

        return new SeedRecipe(
            item.GetProperty("title").GetString()!,
            item.GetProperty("category").GetString()!,
            item.GetProperty("description").GetString()!,
            item.GetProperty("prep").GetInt32(),
            item.GetProperty("cook").GetInt32(),
            item.GetProperty("servings").GetInt32(),
            Enum.Parse<RecipeDifficulty>(item.GetProperty("difficulty").GetString()!, ignoreCase: false),
            ingredients,
            steps);
    }
}
