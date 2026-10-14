namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// Nguyên liệu không thuộc công thức (FR-RCP-009 A1). Như ảnh (RecipeImageNotFoundException), Phụ lục B không có mã riêng
/// cho thành phần con của aggregate Recipe nên dùng lại RECIPE_NOT_FOUND → 404.
/// </summary>
public sealed class RecipeIngredientNotFoundException : RecipeDomainException
{
    public RecipeIngredientNotFoundException(Guid recipeId, Guid ingredientId)
        : base(ErrorCodes.RecipeNotFound, $"Công thức '{recipeId}' không có nguyên liệu '{ingredientId}'.")
    {
        AddExtension("ingredientId", ingredientId);
    }
}

/// <summary>Bước nấu không thuộc công thức (FR-RCP-010 A1) → RECIPE_NOT_FOUND 404.</summary>
public sealed class RecipeStepNotFoundException : RecipeDomainException
{
    public RecipeStepNotFoundException(Guid recipeId, Guid stepId)
        : base(ErrorCodes.RecipeNotFound, $"Công thức '{recipeId}' không có bước '{stepId}'.")
    {
        AddExtension("stepId", stepId);
    }
}
