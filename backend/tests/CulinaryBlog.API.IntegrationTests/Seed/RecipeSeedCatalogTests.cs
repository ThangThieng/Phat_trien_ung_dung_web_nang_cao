using CulinaryBlog.Domain.Common;
using CulinaryBlog.Infrastructure.Persistence.Seed;

namespace CulinaryBlog.API.IntegrationTests.Seed;

/// <summary>SRS §2.6.1 (CR-2026-03): bộ dữ liệu mẫu phải đủ ngưỡng và khớp ràng buộc DB (§7.2 – §7.6).</summary>
public class RecipeSeedCatalogTests
{
    [Fact]
    public void Catalog_HasAtLeastTwentyUniqueCategories()
    {
        var categories = RecipeSeedCatalog.Categories;

        Assert.True(categories.Count >= RecipeSeedCatalog.MinCategories, $"Chỉ có {categories.Count} danh mục.");
        Assert.Equal(categories.Count, categories.Select(c => c.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(categories.Count, categories.Select(c => SlugHelper.Generate(c.Name)).Distinct(StringComparer.Ordinal).Count());
        Assert.All(categories, c => Assert.InRange(c.Name.Length, 1, 100));
    }

    [Fact]
    public void Catalog_HasAtLeastOneHundredRecipesWithUniqueSlugs()
    {
        var recipes = RecipeSeedCatalog.Recipes;

        Assert.True(recipes.Count >= RecipeSeedCatalog.MinRecipes, $"Chỉ có {recipes.Count} công thức.");
        Assert.Equal(recipes.Count, recipes.Select(r => SlugHelper.Generate(r.Title)).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EveryRecipe_HasEnoughIngredientsAndSteps()
    {
        Assert.All(RecipeSeedCatalog.Recipes, r =>
        {
            Assert.True(r.Ingredients.Count >= RecipeSeedCatalog.MinIngredientsPerRecipe, $"{r.Title}: {r.Ingredients.Count} nguyên liệu.");
            Assert.True(r.Steps.Count >= RecipeSeedCatalog.MinStepsPerRecipe, $"{r.Title}: {r.Steps.Count} bước.");
        });
    }

    [Fact]
    public void EveryRecipe_BelongsToACatalogCategory_AndEveryCategoryHasRecipes()
    {
        var names = RecipeSeedCatalog.Categories.Select(c => c.Name).ToHashSet(StringComparer.Ordinal);

        Assert.All(RecipeSeedCatalog.Recipes, r => Assert.Contains(r.Category, names));
        Assert.All(names, n => Assert.Contains(RecipeSeedCatalog.Recipes, r => r.Category == n));
    }

    [Fact]
    public void EveryRecipe_FitsDatabaseConstraints()
    {
        Assert.All(RecipeSeedCatalog.Recipes, r =>
        {
            Assert.InRange(r.Title.Length, 1, 200);
            Assert.False(string.IsNullOrWhiteSpace(r.Description));
            Assert.True(r.PrepTimeMinutes > 0 && r.CookTimeMinutes >= 0 && r.Servings > 0, r.Title);
            Assert.Equal(r.Ingredients.Count, r.Ingredients.Select(i => i.Name).Distinct(StringComparer.Ordinal).Count());
            Assert.All(r.Ingredients, i =>
            {
                Assert.InRange(i.Name.Length, 1, 200);
                Assert.True(i.Unit is null || i.Unit.Length <= 50, i.Name);
                Assert.True(i.Notes is null || i.Notes.Length <= 500, i.Name);
                Assert.True(i.Quantity is null or > 0, i.Name);
            });
            Assert.All(r.Steps, s =>
            {
                Assert.InRange(s.Title.Length, 1, 200);
                Assert.False(string.IsNullOrWhiteSpace(s.Description));
                Assert.True(s.TimerMinutes is null or >= 0, s.Title);
            });
        });
    }
}
