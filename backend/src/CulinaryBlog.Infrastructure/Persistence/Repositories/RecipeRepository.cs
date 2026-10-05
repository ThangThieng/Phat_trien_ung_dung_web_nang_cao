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
    /// <c>IgnoreQueryFilters</c>: <c>IDX_Recipe_Slug</c> hiện là unique THƯỜNG (tính cả công thức đã xóa mềm) — bỏ qua bản
    /// đã xóa thì kiểm tra báo "không trùng" trong khi PostgreSQL vẫn ném 23505. Buổi 4 (D-3) đổi sang partial index thì bỏ dòng này.
    /// </summary>
    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slug);

        return Set
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(r => r.Slug == slug, cancellationToken);
    }
}
