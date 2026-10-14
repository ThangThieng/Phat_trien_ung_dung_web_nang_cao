using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// FR-CAT-003/004/005 – repository GHI cho Category. Kế thừa <c>GetByIdAsync</c>/<c>AddAsync</c> của <see cref="EfRepository{TEntity}"/>.
/// Không gọi SaveChanges ở đây: handler commit qua <see cref="IUnitOfWork"/> để cả thao tác nằm trong một transaction.
/// Không tự cache Redis — cache là việc của CachingBehavior / CacheInvalidationBehavior.
/// </summary>
public sealed class CategoryRepository(CulinaryBlogDbContext db) : EfRepository<Category>(db), ICategoryRepository
{
    /// <summary>Ký tự đặc biệt của LIKE/ILIKE được escape bằng dấu "\" khai báo ở tham số thứ ba.</summary>
    private const string LikeEscape = "\\";

    /// <summary>
    /// So sánh không phân biệt hoa/thường ("Món Chính" trùng "món chính") bằng ILIKE của PostgreSQL.
    /// <c>IgnoreQueryFilters</c>: <c>IDX_Category_Name</c> là unique thường nên danh mục đã soft delete vẫn giữ tên —
    /// nếu bỏ qua chúng thì kiểm tra chủ động báo "không trùng" trong khi PostgreSQL vẫn ném 23505.
    /// </summary>
    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);

        var pattern = EscapeLikePattern(name.Trim());
        var query = Set
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, pattern, LikeEscape));

        if (excludeId is { } currentId)
        {
            query = query.Where(c => c.Id != currentId);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <summary>Cùng lý do với <see cref="ExistsByNameAsync"/>: <c>IDX_Category_Slug</c> cũng là unique thường.</summary>
    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slug);

        return Set
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(c => c.Slug == slug, cancellationToken);
    }

    /// <summary>Đếm mọi trạng thái (Draft/Published/Archived); recipe đã soft delete bị Global Query Filter của Recipe loại sẵn.</summary>
    public Task<int> CountActiveRecipesAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        Db.Recipes
            .AsNoTracking()
            .CountAsync(r => r.CategoryId == categoryId && !r.IsDeleted, cancellationToken);

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
            .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
            .Replace("_", LikeEscape + "_", StringComparison.Ordinal);
}
