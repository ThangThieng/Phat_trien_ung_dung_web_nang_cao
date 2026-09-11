namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.2.1 – Owned Entity, lưu thành các cột "Nutrition_*" trong bảng Recipes. Giá trị tính trên 1 khẩu phần.</summary>
public class RecipeNutrition
{
    public decimal? Calories { get; init; }

    public decimal? Protein { get; init; }

    public decimal? Carbohydrates { get; init; }

    public decimal? Fat { get; init; }

    public decimal? Fiber { get; init; }

    public decimal? Sodium { get; init; }
}
