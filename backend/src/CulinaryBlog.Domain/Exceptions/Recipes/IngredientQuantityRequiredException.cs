namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// INGREDIENT_QUANTITY_REQUIRED — nguyên liệu để rỗng cả <c>quantity</c>, <c>quantityText</c> lẫn <c>unit</c> (FR-RCP-009 A4).
/// Bất biến nằm ở Domain (không ở validator) để MỌI đường thêm nguyên liệu — endpoint riêng, mảng inline của
/// <c>POST /recipes</c>, seeder — cùng tuân thủ một quy tắc.
/// </summary>
public sealed class IngredientQuantityRequiredException : RecipeDomainException
{
    public IngredientQuantityRequiredException(string ingredientName)
        : base(
            ErrorCodes.IngredientQuantityRequired,
            $"Nguyên liệu '{ingredientName}' phải có ít nhất một trong: số lượng, định lượng dạng chữ (ví dụ \"vừa đủ\") hoặc đơn vị.")
    {
        AddExtension("ingredient", ingredientName);
    }
}
