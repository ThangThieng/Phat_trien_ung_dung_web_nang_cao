namespace CulinaryBlog.Domain.Exceptions.Categories;

/// <summary>
/// CATEGORY_NAME_EXISTS — tên danh mục đã tồn tại (FR-CAT-003/004, MT-36). Được ném ở hai nơi: bước kiểm tra chủ động
/// của handler và bộ dịch lỗi ghi DB (lớp phòng vệ thứ hai khi hai Admin ghi cùng tên gần như đồng thời).
/// </summary>
public sealed class CategoryNameAlreadyExistsException : CategoryDomainException
{
    public CategoryNameAlreadyExistsException(string name, Exception? innerException = null)
        : base(ErrorCodes.CategoryNameExists, $"Danh mục '{name}' đã tồn tại.", innerException)
    {
    }
}
