using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// SRS §7.4 – nguyên liệu của công thức; chỉ tạo/sửa qua aggregate <see cref="Recipe"/>. Định lượng hai cột
/// (<see cref="Quantity"/> số, <see cref="QuantityText"/> nguyên văn) — xem <see cref="IngredientDetails"/>.
/// Hiển thị ưu tiên QuantityText, không có thì format Quantity + Unit; scale khẩu phần chỉ áp dụng khi có Quantity.
/// </summary>
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

    internal static RecipeIngredient Create(Guid recipeId, IngredientDetails details, int orderIndex)
    {
        var ingredient = new RecipeIngredient { RecipeId = recipeId };
        ingredient.Apply(details, orderIndex);
        return ingredient;
    }

    internal void Update(IngredientDetails details, int? orderIndex) => Apply(details, orderIndex ?? OrderIndex);

    private void Apply(IngredientDetails details, int orderIndex)
    {
        var valid = details.Validated();
        Name = valid.Name;
        Quantity = valid.Quantity;
        QuantityText = valid.QuantityText;
        Unit = valid.Unit;
        Notes = valid.Notes;
        OrderIndex = orderIndex;
    }
}
