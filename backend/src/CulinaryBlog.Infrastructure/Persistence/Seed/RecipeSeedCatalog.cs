using System.Globalization;
using System.Reflection;
using System.Text.Json;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public sealed record SeedCategory(string Name, string Description);

/// <summary>
/// FR-RCP-009 (Buổi 4): định lượng hai cột — <see cref="Quantity"/> là số tính toán được, <see cref="QuantityText"/> là
/// nguyên văn không quy ra số ("vừa đủ", "vừa ăn"). Mỗi nguyên liệu có ít nhất một trong Quantity/QuantityText/Unit.
/// </summary>
public sealed record SeedIngredient(string Name, decimal? Quantity, string? QuantityText, string? Unit, string? Notes);

public sealed record SeedStep(string Title, string Description, int? TimerMinutes);

/// <summary>Trang nguồn trên Wikibooks Cookbook, ghim theo revision để nội dung không trôi khi wiki đổi (CR-2026-07).</summary>
public sealed record SeedSource(long PageId, long RevisionId, string PageTitle);

/// <summary>Ảnh giấy phép mở: tải về MinIO bằng <c>SeedImageImportJob</c>, không hotlink.</summary>
public sealed record SeedImage(string File, Uri Url, string Artist, string License, Uri DescriptionUrl);

public sealed record SeedRecipe(
    string Title,
    string Category,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    IReadOnlyList<SeedIngredient> Ingredients,
    IReadOnlyList<SeedStep> Steps,
    SeedSource? Source = null,
    SeedImage? Image = null,
    RecipeNutrition? Nutrition = null,
    string? Attribution = null);

/// <summary>
/// Bộ dữ liệu mẫu theo SRS §2.6.1 (CR-2026-03, CR-2026-07): ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và
/// ≥ 5 bước. Gồm 100 công thức do nhóm biên soạn (<c>recipes-0*.json</c>) và các công thức bổ sung từ Wikibooks Cookbook đã dịch
/// (<c>recipes-wikibooks-*.json</c>, sinh bằng <c>tools/recipe-etl</c>, giấy phép CC BY-SA 4.0).
/// Bogus chỉ sinh phân công tác giả, trạng thái của công thức do nhóm biên soạn và ngày xuất bản (tác giả mẫu mang tên "Tác giả mẫu 1…5")
/// trong <see cref="DatabaseSeeder"/>; dinh dưỡng không còn sinh ngẫu nhiên.
/// </summary>
public static class RecipeSeedCatalog
{
    /// <summary>
    /// Tên 5 tác giả mẫu (SRS §2.6.1). Từ 19/11/2026 ghi rõ là tài khoản mẫu — trước đó Bogus đặt tên người Việt ngẫu nhiên
    /// ("Kim Quyên Vũ"…) khiến người xem tưởng là người thật, trái yêu cầu dữ liệu thật của trưởng nhóm.
    /// </summary>
    public const string SampleAuthorBio = "Tài khoản mẫu để trình diễn hệ thống — không phải người thật.";

    public const int MinCategories = 20;
    public const int MinRecipes = 100;
    public const int MinIngredientsPerRecipe = 10;
    public const int MinStepsPerRecipe = 5;

    public static string SampleAuthorName(int index) => $"Tác giả mẫu {index}";

    /// <summary>Tiền tố của chuỗi ghi công trong cột Instructions; Frontend chỉ hiển thị khối nguồn khi có tiền tố này.</summary>
    public const string SourceNotePrefix = "Nguồn dữ liệu:";

    private const string ResourcePrefix = "SeedData/";
    private static readonly Lazy<(IReadOnlyList<SeedCategory> Categories, IReadOnlyList<SeedRecipe> Recipes)> Data = new(Load);
    private static readonly Lazy<Dictionary<string, SeedImage>> LegacyImageData = new(LoadLegacyImages);
    private static readonly Lazy<IReadOnlyDictionary<string, SeedRecipe>> RecipesBySlug = new(() =>
        Data.Value.Recipes.ToDictionary(r => SlugHelper.Generate(r.Title), StringComparer.Ordinal));

    public static IReadOnlyList<SeedCategory> Categories => Data.Value.Categories;

    public static IReadOnlyList<SeedRecipe> Recipes => Data.Value.Recipes;

    /// <summary>Ảnh đã xem bằng mắt là đúng món, cho công thức do nhóm biên soạn. Khóa là slug.</summary>
    public static IReadOnlyDictionary<string, SeedImage> LegacyImages => LegacyImageData.Value;

    public static SeedRecipe? FindBySlug(string slug) => RecipesBySlug.Value.GetValueOrDefault(slug);

    /// <summary>Ảnh cần nhập cho một slug: ảnh của công thức Wikibooks, nếu không có thì ảnh đã duyệt của công thức cũ.</summary>
    public static SeedImage? FindImage(string slug) =>
        FindBySlug(slug)?.Image ?? LegacyImages.GetValueOrDefault(slug);

    /// <summary>Ghi công ảnh cho công thức do nhóm biên soạn: nội dung của nhóm, chỉ ảnh là từ nguồn ngoài.</summary>
    public static string LegacyImageCredit(SeedImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return string.Create(CultureInfo.InvariantCulture, $"{SourceNotePrefix} Ảnh: {image.Artist}, {image.License}, {image.DescriptionUrl.AbsoluteUri}.");
    }

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
        // Mỗi nguyên liệu là mảng gồm tên, định lượng, đơn vị, ghi chú (tùy chọn). Định lượng dạng số đi vào Quantity,
        // dạng chữ ("vừa đủ") đi vào QuantityText. Mỗi bước là mảng gồm tiêu đề, mô tả, số phút (có thể null).
        var ingredients = item.GetProperty("ingredients").EnumerateArray()
            .Select(i => new SeedIngredient(
                i[0].GetString()!,
                i[1].ValueKind == JsonValueKind.Number ? i[1].GetDecimal() : null,
                i[1].ValueKind == JsonValueKind.String ? i[1].GetString() : null,
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
            steps,
            item.TryGetProperty("source", out var source) ? ParseSource(source) : null,
            item.TryGetProperty("image", out var image) ? ParseImage(image) : null,
            item.TryGetProperty("nutrition", out var nutrition) ? ParseNutrition(nutrition) : null,
            item.TryGetProperty("attribution", out var attribution) ? attribution.GetString() : null);
    }

    private static SeedSource ParseSource(JsonElement item) =>
        new(item.GetProperty("pageId").GetInt64(), item.GetProperty("revisionId").GetInt64(), item.GetProperty("title").GetString()!);

    private static SeedImage ParseImage(JsonElement item) =>
        new(
            item.GetProperty("file").GetString()!,
            new Uri(item.GetProperty("url").GetString()!),
            item.GetProperty("artist").GetString()!,
            item.GetProperty("license").GetString()!,
            new Uri(item.GetProperty("descriptionUrl").GetString()!));

    /// <summary>ETL chỉ ghi dinh dưỡng khi đủ 6 chỉ số và qua kiểm tra hợp lý; còn lại là null.</summary>
    private static RecipeNutrition? ParseNutrition(JsonElement item) =>
        item.ValueKind == JsonValueKind.Null
            ? null
            : new RecipeNutrition
            {
                Calories = item.GetProperty("calories").GetDecimal(),
                Protein = item.GetProperty("protein").GetDecimal(),
                Carbohydrates = item.GetProperty("carbohydrates").GetDecimal(),
                Fat = item.GetProperty("fat").GetDecimal(),
                Fiber = item.GetProperty("fiber").GetDecimal(),
                Sodium = item.GetProperty("sodium").GetDecimal(),
            };

    private static Dictionary<string, SeedImage> LoadLegacyImages()
    {
        using var document = Read(typeof(RecipeSeedCatalog).Assembly, $"{ResourcePrefix}legacy-recipe-images.json");
        return document.RootElement.EnumerateArray()
            .ToDictionary(i => i.GetProperty("slug").GetString()!, ParseImage, StringComparer.Ordinal);
    }
}
