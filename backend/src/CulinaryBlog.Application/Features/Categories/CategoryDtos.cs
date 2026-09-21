using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>FR-CAT-001 – { id, name, slug, description, imageUrl, recipeCount } (recipeCount chỉ đếm Published).</summary>
public sealed record CategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl, int OrderIndex, int RecipeCount);

/// <summary>FR-CAT-002 – { category, recipes: PagedResult }.</summary>
public sealed record CategoryDetailDto(CategoryDto Category, PagedResult<RecipeSummaryDto> Recipes);

public interface ICategoryReadRepository
{
    Task<IReadOnlyList<CategoryDto>> GetAllWithRecipeCountAsync(CancellationToken cancellationToken);

    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}

public static class CategoryCacheKeys
{
    /// <summary>Key theo FR-CAT-001 bước 3. Command Create/Update/Delete (Buổi 3) sẽ invalidate key này.</summary>
    public const string All = "categories:all";
}
/// <summary>
/// FR-CAT-003/004/005 – repository GHI cho Category. Application không được tham chiếu EF Core (CONS-001),
/// nên mọi truy cập dữ liệu đi qua interface này; việc commit do <see cref="Common.Interfaces.IUnitOfWork"/> đảm nhiệm.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra trùng Name CHỦ ĐỘNG trước khi ghi (MT-36 – không để UNIQUE của DB bắt hộ rồi thành 500).
    /// <paramref name="excludeId"/> bỏ qua chính bản ghi đang sửa, phục vụ FR-CAT-004.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>Mọi slug bắt đầu bằng <paramref name="baseSlug"/> – đủ để chọn hậu tố -2, -3 trong MỘT round-trip.</summary>
    Task<IReadOnlyCollection<string>> GetSlugsStartingWithAsync(string baseSlug, CancellationToken cancellationToken);

    /// <summary>Số công thức chưa soft-delete đang trỏ tới danh mục – mọi trạng thái Draft/Published/Archived (FR-CAT-005).</summary>
    Task<int> CountRecipesAsync(Guid categoryId, CancellationToken cancellationToken);

    Task AddAsync(Category category, CancellationToken cancellationToken);
}

/// <summary>
/// SRS §7.9 Bảng Giới hạn Dữ liệu Chuẩn (MT-37) – nguồn sự thật duy nhất cho độ dài field Category.
/// Validator và định nghĩa cột DB PHẢI bằng nhau: Name varchar(100), Slug varchar(120), ImageUrl varchar(500).
/// </summary>
public static class CategoryLimits
{
    public const int NameMin = 2;
    public const int NameMax = 100;
    public const int DescriptionMax = 2000;
    public const int ImageUrlMax = 500;
}
