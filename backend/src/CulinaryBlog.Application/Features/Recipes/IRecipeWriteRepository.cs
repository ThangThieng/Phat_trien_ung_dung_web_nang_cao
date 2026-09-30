using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Features.Recipes;

public interface IRecipeWriteRepository
{
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(Recipe recipe, CancellationToken cancellationToken);
    Task<Recipe?> GetForWriteAsync(Guid recipeId, CancellationToken cancellationToken);
    void RemoveImage(RecipeImage image);
}
