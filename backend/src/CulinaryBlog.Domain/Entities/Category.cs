using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.6 – danh mục công thức.</summary>
public class Category : BaseEntity
{
    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ImageUrl { get; private set; }

    public int OrderIndex { get; private set; }

    public static Category Create(string name, string slug, string? description = null, string? imageUrl = null, int orderIndex = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category
        {
            Name = name.Trim(),
            Slug = slug,
            Description = description,
            ImageUrl = imageUrl,
            OrderIndex = orderIndex,
        };
    }
}
