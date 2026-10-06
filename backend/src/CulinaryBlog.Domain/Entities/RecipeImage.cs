using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.5 – ảnh công thức trên MinIO. Mỗi Recipe chỉ có 1 ảnh IsPrimary (partial unique index, chỉ tính ảnh chưa xóa).</summary>
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
            AltText = altText?.Trim(),
            IsPrimary = isPrimary,
            OrderIndex = orderIndex,
        };

    /// <summary>PATCH metadata (FR-RCP-008 bước 10): chỉ đổi trường được gửi — <c>null</c> nghĩa là "giữ nguyên".</summary>
    public void UpdateMetadata(string? altText, int? orderIndex)
    {
        if (altText is not null)
        {
            AltText = altText.Trim();
        }

        if (orderIndex.HasValue)
        {
            OrderIndex = orderIndex.Value;
        }
    }

    /// <summary>FR-JOB-002: ghi URL hai biến thể do job đổi kích thước sinh ra.</summary>
    public void SetResizedVariants(string mediumUrl, string thumbnailUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediumUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(thumbnailUrl);

        MediumUrl = mediumUrl;
        ThumbnailUrl = thumbnailUrl;
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    /// <summary>FR-RCP-008 bước 14: xóa mềm (IsDeleted = true) và rời vai trò ảnh chính.</summary>
    internal void SoftDelete()
    {
        IsDeleted = true;
        IsPrimary = false;
    }
}
