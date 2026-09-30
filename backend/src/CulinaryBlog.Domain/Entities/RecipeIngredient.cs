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

    public string? QuantityText { get; private set; }

    public string? Unit { get; private set; }

    public string? Notes { get; private set; }

    public int OrderIndex { get; private set; }

    public static RecipeIngredient Create(Guid recipeId, string name, decimal? quantity, string? unit, string? notes = null, int orderIndex = 0, string? quantityText = null)
    {
        if (quantity is null && string.IsNullOrWhiteSpace(quantityText) && string.IsNullOrWhiteSpace(unit))
        {
            throw new DomainException("Phải có Quantity, QuantityText hoặc Unit.");
        }

        return new()
        {
            RecipeId = recipeId,
            Name = name,
            Quantity = quantity,
            QuantityText = string.IsNullOrWhiteSpace(quantityText) ? null : quantityText.Trim(),
            Unit = unit,
            Notes = notes,
            OrderIndex = orderIndex,
        };
    }

    public void Update(string name, decimal? quantity, string? unit, string? notes, string? quantityText)
    {
        if (quantity is null && string.IsNullOrWhiteSpace(quantityText) && string.IsNullOrWhiteSpace(unit))
        {
            throw new DomainException("Phải có Quantity, QuantityText hoặc Unit.");
        }

        Name = name.Trim();
        Quantity = quantity;
        Unit = unit?.Trim();
        Notes = notes?.Trim();
        QuantityText = string.IsNullOrWhiteSpace(quantityText) ? null : quantityText.Trim();
    }
}
