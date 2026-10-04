namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// Ảnh không tồn tại trong công thức (FR-RCP-008). Phụ lục B không có mã riêng cho ảnh nên dùng lại RECIPE_NOT_FOUND:
/// ảnh là thành phần của aggregate Recipe và Frontend xử lý hai trường hợp như nhau (tải lại trang công thức).
/// </summary>
public sealed class RecipeImageNotFoundException : RecipeDomainException
{
    public RecipeImageNotFoundException(Guid recipeId, Guid imageId)
        : base(ErrorCodes.RecipeNotFound, $"Công thức '{recipeId}' không có ảnh '{imageId}'.")
    {
        AddExtension("imageId", imageId);
    }
}
