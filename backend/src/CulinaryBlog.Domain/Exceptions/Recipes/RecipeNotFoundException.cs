namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// RECIPE_NOT_FOUND — không có công thức (chưa xóa mềm) với id hoặc slug đã cho. Cũng dùng khi công thức tồn tại nhưng
/// người gọi không được thấy nó ở endpoint đó (Draft qua endpoint công khai — MT-34), để không lộ sự tồn tại của bản nháp.
/// </summary>
public sealed class RecipeNotFoundException : RecipeDomainException
{
    public RecipeNotFoundException(Guid id)
        : base(ErrorCodes.RecipeNotFound, $"Không tìm thấy công thức với id '{id}'.")
    {
    }

    public RecipeNotFoundException(string slug)
        : base(ErrorCodes.RecipeNotFound, $"Không tìm thấy công thức '{slug}'.")
    {
    }
}
