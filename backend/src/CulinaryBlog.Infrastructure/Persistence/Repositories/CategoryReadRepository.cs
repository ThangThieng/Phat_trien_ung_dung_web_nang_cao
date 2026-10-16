using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class CategoryReadRepository(CulinaryBlogDbContext db) : ICategoryReadRepository
{
    /// <summary>FR-CAT-001 – SRS §8.2: sắp xếp OrderIndex tăng dần rồi Name tăng dần, recipeCount chỉ đếm Published (correlated subquery, 1 round-trip).</summary>
    public async Task<IReadOnlyList<CategoryDto>> GetAllWithRecipeCountAsync(CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.OrderIndex)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ImageUrl,
                c.OrderIndex,
                db.Recipes.Count(r => r.CategoryId == c.Id && r.Status == RecipeStatus.Published)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        db.Categories
            .AsNoTracking()
            .Where(c => c.Slug == slug)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ImageUrl,
                c.OrderIndex,
                db.Recipes.Count(r => r.CategoryId == c.Id && r.Status == RecipeStatus.Published)))
            .FirstOrDefaultAsync(cancellationToken);
}
