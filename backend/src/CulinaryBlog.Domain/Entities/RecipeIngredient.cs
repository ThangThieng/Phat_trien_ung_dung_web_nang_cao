using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.4 – nguyên liệu của công thức.</summary>
public class RecipeIngredient : BaseEntity
{
    private RecipeIngredient()
    {
    }

    public Guid RecipeId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal? Quantity { get; private set; }

    public string? Unit { get; private set; }

    public string? Notes { get; private set; }

    public int OrderIndex { get; private set; }

    public static RecipeIngredient Create(Guid recipeId, string name, decimal? quantity, string? unit, string? notes = null, int orderIndex = 0) =>
        new()
        {
            RecipeId = recipeId,
            Name = name,
            Quantity = quantity,
            Unit = unit,
            Notes = notes,
            OrderIndex = orderIndex,
        };
}
