using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// FR-CAT-003/004/005 – repository ghi cho Category. Không gọi SaveChanges ở đây:
/// handler commit qua IUnitOfWork để cả thao tác nằm trong một transaction (cùng style RefreshTokenRepository).
/// Mọi truy vấn đi qua Global Query Filter IsDeleted = false của BaseEntity.
/// </summary>
public sealed class CategoryRepository(CulinaryBlogDbContext db) : ICategoryRepository
{
    /// <summary>Ký tự đặc biệt của LIKE/ILIKE được escape bằng dấu "\" khai báo ở tham số thứ ba.</summary>
    private const string LikeEscape = "\\";

    /// <summary>Tracked (không AsNoTracking) vì entity trả về sẽ được Update()/soft delete rồi commit.</summary>
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    /// <summary>
    /// So sánh không phân biệt hoa/thường ("Món Chính" trùng "món chính") bằng ILIKE của PostgreSQL.
    /// Dùng ILIKE thay cho ToLower() vì ToLower() trong cây biểu thức LINQ kích analyzer CA1862.
    /// </summary>
    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(name);

        var pattern = EscapeLikePattern(name.Trim());
        var query = db.Categories
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, pattern, LikeEscape));

        if (excludeId is { } currentId)
        {
            query = query.Where(c => c.Id != currentId);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <summary>Mọi slug bắt đầu bằng <paramref name="baseSlug"/> – đủ để chọn hậu tố -2, -3 trong MỘT round-trip.</summary>
    public async Task<IReadOnlyCollection<string>> GetSlugsStartingWithAsync(string baseSlug, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(baseSlug);

        var pattern = EscapeLikePattern(baseSlug) + "%";
        return await db.Categories
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.Slug, pattern, LikeEscape))
            .Select(c => c.Slug)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Đếm mọi trạng thái (Draft/Published/Archived); recipe đã soft-delete bị Global Query Filter loại sẵn.</summary>
    public Task<int> CountRecipesAsync(Guid categoryId, CancellationToken cancellationToken) =>
        db.Recipes.CountAsync(r => r.CategoryId == categoryId, cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken) =>
        await db.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
            .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
            .Replace("_", LikeEscape + "_", StringComparison.Ordinal);
}
