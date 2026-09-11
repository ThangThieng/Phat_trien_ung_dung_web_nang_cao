using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Enums;

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

    public string Instructions { get; private set; } = string.Empty;

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

    public IReadOnlyCollection<RecipeImage> Images => _images.AsReadOnly();

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
            throw new DomainException("PrepTime và Servings phải > 0, CookTime phải >= 0.");
        }

        return new Recipe
        {
            Title = title.Trim(),
            Slug = slug,
            Description = description,
            Instructions = instructions ?? string.Empty,
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

    public RecipeImage AddImage(string originalUrl, string? altText)
    {
        // FR-RCP-008: ảnh đầu tiên tự động là ảnh chính
        var image = RecipeImage.Create(Id, originalUrl, altText, isPrimary: _images.Count == 0, _images.Count);
        _images.Add(image);
        return image;
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
            throw new DomainException("Recipe phải có ít nhất 1 bước thực hiện và 1 nguyên liệu.");
        }

        Status = RecipeStatus.Published;
        PublishedAt ??= utcNow;
    }
}
