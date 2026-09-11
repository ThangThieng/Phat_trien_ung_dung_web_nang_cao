using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.5 – ảnh công thức trên MinIO. Mỗi Recipe chỉ có 1 ảnh IsPrimary.</summary>
public class RecipeImage : BaseEntity
{
    private RecipeImage()
    {
    }

    public Guid RecipeId { get; private set; }

    public string OriginalUrl { get; private set; } = string.Empty;

    public string? MediumUrl { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    public string? AltText { get; private set; }

    public bool IsPrimary { get; private set; }

    public int OrderIndex { get; private set; }

    public static RecipeImage Create(Guid recipeId, string originalUrl, string? altText, bool isPrimary, int orderIndex = 0) =>
        new()
        {
            RecipeId = recipeId,
            OriginalUrl = originalUrl,
            AltText = altText,
            IsPrimary = isPrimary,
            OrderIndex = orderIndex,
        };
}
