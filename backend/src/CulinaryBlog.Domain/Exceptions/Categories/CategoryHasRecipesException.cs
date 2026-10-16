namespace CulinaryBlog.Domain.Exceptions.Categories;

/// <summary>
/// CATEGORY_DELETE_HAS_RECIPES — không xóa được danh mục còn công thức (FR-CAT-005). Số lượng vừa nằm trong câu thông báo
/// vừa là extension <c>recipeCount</c> để Frontend hiển thị mà không phải tách số từ một chuỗi tiếng Việt.
/// </summary>
public sealed class CategoryHasRecipesException : CategoryDomainException
{
    public CategoryHasRecipesException(int recipeCount)
        : base(
            ErrorCodes.CategoryDeleteHasRecipes,
            $"Danh mục đang có {recipeCount} công thức. Hãy chuyển chúng sang danh mục khác trước khi xóa.")
    {
        RecipeCount = recipeCount;
        AddExtension("recipeCount", recipeCount);
    }

    public int RecipeCount { get; }
}
