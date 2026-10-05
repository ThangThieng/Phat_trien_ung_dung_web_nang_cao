using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.2 – Aggregate Root: chứa Steps, Ingredients, Images và Owned Entity Nutrition.</summary>
public class Recipe : BaseEntity
{
    private readonly List<RecipeStep> _steps = [];
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeImage> _images = [];

    private Recipe()
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? Instructions { get; private set; }

    public int PrepTimeMinutes { get; private set; }

    public int CookTimeMinutes { get; private set; }

    public int Servings { get; private set; }

    public RecipeDifficulty Difficulty { get; private set; } = RecipeDifficulty.Easy;

    public RecipeStatus Status { get; private set; } = RecipeStatus.Draft;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public string AuthorId { get; private set; } = string.Empty;

    public DateTime? PublishedAt { get; private set; }

    public RecipeNutrition? Nutrition { get; private set; }

    public IReadOnlyCollection<RecipeStep> Steps => _steps.AsReadOnly();

    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    /// <summary>
    /// Ảnh chưa bị xóa. Ảnh xóa mềm (FR-RCP-008 bước 14) vẫn nằm trong backing field cho tới lần lưu kế tiếp: gỡ nó khỏi
    /// collection thì EF coi là bản ghi mồ côi và XÓA CỨNG — trái yêu cầu IsDeleted = true của SRS.
    /// </summary>
    public IReadOnlyCollection<RecipeImage> Images => _images.Where(i => !i.IsDeleted).ToList().AsReadOnly();

    public static Recipe Create(
        string title,
        string slug,
        string description,
        Guid categoryId,
        string authorId,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        RecipeDifficulty difficulty,
        string? instructions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);

        if (prepTimeMinutes <= 0 || cookTimeMinutes < 0 || servings <= 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "PrepTime và Servings phải > 0, CookTime phải >= 0.");
        }

        return new Recipe
        {
            Title = title.Trim(),
            Slug = slug,
            Description = description,
            Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim(),
            CategoryId = categoryId,
            AuthorId = authorId,
            PrepTimeMinutes = prepTimeMinutes,
            CookTimeMinutes = cookTimeMinutes,
            Servings = servings,
            Difficulty = difficulty,
            Status = RecipeStatus.Draft,
        };
    }

    public RecipeStep AddStep(string title, string description, int? timerMinutes = null, string? imageUrl = null)
    {
        var nextNumber = _steps.Count == 0 ? 1 : _steps.Max(s => s.StepNumber) + 1;
        var step = RecipeStep.Create(Id, nextNumber, title, description, timerMinutes, imageUrl);
        _steps.Add(step);
        return step;
    }

    public RecipeIngredient AddIngredient(string name, decimal? quantity, string? unit, string? notes = null)
    {
        var ingredient = RecipeIngredient.Create(Id, name, quantity, unit, notes, _ingredients.Count);
        _ingredients.Add(ingredient);
        return ingredient;
    }

    public RecipeImage GetImage(Guid imageId) =>
        _images.SingleOrDefault(i => i.Id == imageId && !i.IsDeleted) ?? throw new RecipeImageNotFoundException(Id, imageId);

    /// <summary>
    /// FR-RCP-008 bước 6: ảnh đầu tiên tự động là ảnh chính; <paramref name="isPrimary"/> = true hạ mọi ảnh khác xuống
    /// (bất biến "chỉ 1 ảnh chính"). <paramref name="orderIndex"/> mặc định là cuối danh sách.
    /// </summary>
    public RecipeImage AddImage(string originalUrl, string? altText, bool? isPrimary = null, int? orderIndex = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalUrl);

        var active = Images;
        var primary = isPrimary ?? active.Count == 0;
        if (primary)
        {
            DemoteAllImages();
        }

        var image = RecipeImage.Create(Id, originalUrl, altText, primary, orderIndex ?? active.Count);
        _images.Add(image);
        return image;
    }

    /// <summary>PATCH metadata ảnh (FR-RCP-008 bước 9–11). Đổi ảnh chính đi qua <see cref="SetPrimaryImage"/>.</summary>
    public RecipeImage UpdateImageMetadata(Guid imageId, string? altText, int? orderIndex)
    {
        var image = GetImage(imageId);
        image.UpdateMetadata(altText, orderIndex);
        return image;
    }

    /// <summary>
    /// Hạ mọi ảnh chính. ⚠️ <c>IDX_RecipeImage_Primary</c> là partial unique index — PostgreSQL không cho index là
    /// DEFERRABLE và EF không bảo đảm thứ tự hai lệnh UPDATE trong một batch, nên handler phải LƯU bước hạ này trước rồi
    /// mới nâng ảnh mới (hai SaveChanges trong một transaction).
    /// </summary>
    /// <returns><c>true</c> nếu trước đó có ảnh chính.</returns>
    public bool DemoteAllImages()
    {
        var hadPrimary = false;
        foreach (var image in _images.Where(i => i.IsPrimary))
        {
            image.SetPrimary(false);
            hadPrimary = true;
        }

        return hadPrimary;
    }

    /// <summary>Đặt <paramref name="imageId"/> làm ảnh chính, hạ mọi ảnh khác (quy tắc 1 của SRS §8.4).</summary>
    public RecipeImage SetPrimaryImage(Guid imageId)
    {
        var image = GetImage(imageId);
        DemoteAllImages();
        image.SetPrimary(true);
        return image;
    }

    /// <summary>Xóa mềm ảnh (FR-RCP-008 bước 14). Chọn ảnh chính thay thế ở <see cref="PromoteFallbackPrimaryImage"/> sau khi đã lưu.</summary>
    public RecipeImage RemoveImage(Guid imageId)
    {
        var image = GetImage(imageId);
        image.SoftDelete();
        return image;
    }

    /// <summary>
    /// Quy tắc 2 của SRS §8.4: không còn ảnh chính mà vẫn còn ảnh → ảnh có OrderIndex nhỏ nhất (hòa thì CreatedAt sớm
    /// nhất) lên làm ảnh chính. Tiêu chí xác định rõ ràng để không phụ thuộc thứ tự trả về của DB.
    /// </summary>
    public RecipeImage? PromoteFallbackPrimaryImage()
    {
        var active = _images.Where(i => !i.IsDeleted).ToList();
        if (active.Count == 0 || active.Exists(i => i.IsPrimary))
        {
            return null;
        }

        var next = active.OrderBy(i => i.OrderIndex).ThenBy(i => i.CreatedAt).First();
        next.SetPrimary(true);
        return next;
    }

    public void SetNutrition(RecipeNutrition? nutrition) => Nutrition = nutrition;

    /// <summary>FR-RCP-005: phải có ít nhất 1 bước và 1 nguyên liệu (Phụ lục B – RECIPE_PUBLISH_INCOMPLETE).</summary>
    public void Publish(DateTime utcNow)
    {
        if (Status == RecipeStatus.Published)
        {
            return;
        }

        if (_steps.Count == 0 || _ingredients.Count == 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.RecipePublishIncomplete, "Recipe phải có ít nhất 1 bước thực hiện và 1 nguyên liệu.");
        }

        Status = RecipeStatus.Published;
        PublishedAt ??= utcNow;
    }
}
