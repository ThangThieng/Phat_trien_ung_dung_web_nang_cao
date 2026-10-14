using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// Nội dung một nguyên liệu (FR-RCP-009, MT-04) — chiến lược HAI cột định lượng: <see cref="Quantity"/> là số tính toán được
/// (scale khẩu phần, recipeIngredient của JSON-LD), <see cref="QuantityText"/> giữ nguyên văn không quy ra số
/// ("1/2 muỗng", "vừa đủ"). Gom thành một record để thêm cột không làm lệch vị trí tham số ở nơi gọi.
/// </summary>
public sealed record IngredientDetails(string Name, decimal? Quantity, string? QuantityText, string? Unit, string? Notes)
{
    /// <summary>
    /// Chuẩn hóa (trim, chuỗi trắng → null) rồi kiểm tra bất biến: tên không rỗng, Quantity nếu có phải &gt; 0, và
    /// KHÔNG được rỗng cả Quantity, QuantityText lẫn Unit (INGREDIENT_QUANTITY_REQUIRED — ràng buộc mềm thay cho
    /// "Quantity bắt buộc" của v1.0.0, vốn mâu thuẫn với cột NULL).
    /// </summary>
    internal IngredientDetails Validated()
    {
        var normalized = new IngredientDetails(Name?.Trim() ?? string.Empty, Quantity, Blank(QuantityText), Blank(Unit), Blank(Notes));

        if (normalized.Name.Length == 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "Tên nguyên liệu không được để trống.");
        }

        if (normalized.Quantity is <= 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "Số lượng nguyên liệu phải lớn hơn 0.");
        }

        if (normalized.Quantity is null && normalized.QuantityText is null && normalized.Unit is null)
        {
            throw new IngredientQuantityRequiredException(normalized.Name);
        }

        return normalized;
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
