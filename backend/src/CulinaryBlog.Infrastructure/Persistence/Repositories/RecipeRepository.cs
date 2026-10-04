using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// <see cref="IRecipeRepository"/> — repository GHI của aggregate Recipe (Buổi 3 — Dev 2). Trả entity được EF theo dõi;
/// không gọi SaveChanges (handler commit qua IUnitOfWork). Global Query Filter áp dụng cho cả collection con, nên ảnh/bước
/// đã xóa mềm không được nạp.
/// </summary>
public sealed class RecipeRepository(CulinaryBlogDbContext db) : EfRepository<Recipe>(db), IRecipeRepository
{
    /// <summary>
    /// AsSplitQuery: ba collection trong một JOIN sinh tích Descartes (10 nguyên liệu × 5 bước × 4 ảnh = 200 dòng cho một
    /// công thức). Nutrition là Owned Entity nằm cùng dòng Recipes nên không cần Include.
    /// </summary>
    public Task<Recipe?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Recipe?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <summary>
    /// D-3 (Buổi 4): <c>IDX_Recipe_Slug</c> là partial unique (<c>WHERE "IsDeleted" = false</c>) nên chỉ công thức CHƯA xóa
    /// giữ slug — Global Query Filter loại bản đã xóa mềm đúng như index. (Buổi 3 phải dùng IgnoreQueryFilters vì index
    /// còn là unique thường.)
    /// </summary>
    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slug);

        return Set
            .AsNoTracking()
            .AnyAsync(r => r.Slug == slug, cancellationToken);
    }
}
