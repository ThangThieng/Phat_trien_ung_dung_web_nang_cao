using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

/// <summary>
/// FR-CAT-003/004/005 – repository GHI cho Category (SRS §6.2: interface ở Application, cài đặt ở Infrastructure).
/// Kế thừa <c>GetByIdAsync</c> và <c>AddAsync</c> từ <see cref="IRepository{TEntity}"/>. Phía đọc/hiển thị
/// (danh sách, chi tiết, recipeCount) nằm ở <c>ICategoryReadRepository</c>. Không tự cache Redis: cache là việc của
/// <c>CachingBehavior</c> / <c>CacheInvalidationBehavior</c>. Commit qua <see cref="IUnitOfWork"/>.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Trùng Name (không phân biệt hoa/thường), <b>tính cả danh mục đã soft delete</b>: <c>IDX_Category_Name</c> là
    /// unique thường nên bản ghi đã xóa mềm vẫn giữ tên — bỏ qua chúng thì kiểm tra nói "không trùng" nhưng DB vẫn ném 23505.
    /// <paramref name="excludeId"/> bỏ qua chính bản ghi đang sửa (FR-CAT-004).
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default);

    /// <summary>Slug đã có người giữ chưa — cũng tính cả danh mục đã soft delete vì <c>IDX_Category_Slug</c> là unique thường.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Số công thức chưa soft delete đang trỏ tới danh mục — mọi trạng thái Draft/Published/Archived (FR-CAT-005).</summary>
    Task<int> CountActiveRecipesAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
