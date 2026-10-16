namespace CulinaryBlog.Domain.Exceptions.Categories;

/// <summary>CATEGORY_NOT_FOUND — không có danh mục (chưa soft delete) với id hoặc slug đã cho (FR-CAT-002/004/005).</summary>
public sealed class CategoryNotFoundException : CategoryDomainException
{
    public CategoryNotFoundException(Guid id)
        : base(ErrorCodes.CategoryNotFound, $"Không tìm thấy danh mục với id '{id}'.")
    {
    }

    public CategoryNotFoundException(string slug)
        : base(ErrorCodes.CategoryNotFound, $"Không tìm thấy danh mục '{slug}'.")
    {
    }
}
