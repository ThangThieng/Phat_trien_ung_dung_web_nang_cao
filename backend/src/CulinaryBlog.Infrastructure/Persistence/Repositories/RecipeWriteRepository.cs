using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class RecipeWriteRepository(CulinaryBlogDbContext db) : IRecipeWriteRepository
{
    public Task<bool> CategoryExistsAsync(Guid id, CancellationToken ct) => db.Categories.AnyAsync(x => x.Id == id, ct);
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) => db.Recipes.AnyAsync(x => x.Slug == slug, ct);
    public Task AddAsync(Recipe recipe, CancellationToken ct) => db.Recipes.AddAsync(recipe, ct).AsTask();
    public Task<Recipe?> GetForWriteAsync(Guid id, CancellationToken ct) => db.Recipes.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
    public void RemoveImage(RecipeImage image) => db.RecipeImages.Remove(image);
}
