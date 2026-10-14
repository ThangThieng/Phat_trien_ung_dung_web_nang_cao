using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Dịch lỗi ghi DB của aggregate Recipe (Buổi 3 — Dev 2, SRS v1.2.2 MT-63):
/// • <see cref="DbUpdateConcurrencyException"/> có entry thuộc Recipe/RecipeStep/RecipeIngredient/RecipeImage →
///   <see cref="RecipeConcurrencyException"/> (409 RECIPE_CONCURRENCY_CONFLICT) — nền của FR-RCP-004 (PUT có RowVersion).
/// • 23505 trên <c>IDX_Recipe_Slug</c> → <see cref="RecipeSlugConflictException"/> (409 RECIPE_SLUG_EXISTS) — hai request
///   tranh cùng slug cùng vượt qua bước kiểm tra chủ động.
/// Handler không bao giờ phải bắt exception của EF (Application không tham chiếu EF).
/// </summary>
internal sealed class RecipePersistenceExceptionTranslator : IPersistenceExceptionTranslator
{
    internal const string SlugIndexName = "IDX_Recipe_Slug";

    public DomainException? TryTranslate(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is DbUpdateConcurrencyException concurrency)
        {
            var recipeEntry = concurrency.Entries.FirstOrDefault(e => RecipeIdOf(e.Entity) is not null);
            return recipeEntry is null ? null : new RecipeConcurrencyException(RecipeIdOf(recipeEntry.Entity), concurrency);
        }

        if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres
            && postgres.ConstraintName == SlugIndexName)
        {
            var slug = exception.Entries.Select(e => e.Entity).OfType<Recipe>().Select(r => r.Slug).FirstOrDefault();
            return new RecipeSlugConflictException(slug ?? "(không rõ)", exception);
        }

        return null;
    }

    /// <summary>Id của công thức sở hữu entity (chính nó, hoặc RecipeId của thành phần con); null nếu không thuộc aggregate Recipe.</summary>
    private static Guid? RecipeIdOf(object entity) => entity switch
    {
        Recipe recipe => recipe.Id,
        RecipeStep step => step.RecipeId,
        RecipeIngredient ingredient => ingredient.RecipeId,
        RecipeImage image => image.RecipeId,
        _ => null,
    };
}
