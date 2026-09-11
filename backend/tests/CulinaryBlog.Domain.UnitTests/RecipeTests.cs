using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.UnitTests;

public class RecipeTests
{
    [Fact]
    public void Create_StartsAsDraft()
    {
        var recipe = NewRecipe();

        Assert.Equal(RecipeStatus.Draft, recipe.Status);
        Assert.Null(recipe.PublishedAt);
    }

    [Fact]
    public void Create_RejectsInvalidTimes() =>
        Assert.Throws<DomainException>(() =>
            Recipe.Create("Phở bò", "pho-bo", "Mô tả", Guid.NewGuid(), "author-1", 0, 60, 4, RecipeDifficulty.Easy));

    [Fact]
    public void AddStep_AssignsSequentialStepNumbers()
    {
        var recipe = NewRecipe();

        recipe.AddStep("Sơ chế", "...");
        recipe.AddStep("Nấu", "...");

        Assert.Equal([1, 2], recipe.Steps.Select(s => s.StepNumber));
    }

    [Fact]
    public void AddImage_FirstImageIsPrimary()
    {
        var recipe = NewRecipe();

        recipe.AddImage("http://minio/a.jpg", null);
        recipe.AddImage("http://minio/b.jpg", null);

        Assert.Single(recipe.Images, i => i.IsPrimary);
        Assert.True(recipe.Images.First().IsPrimary);
    }

    [Fact]
    public void Publish_WithoutStepsOrIngredients_Throws()
    {
        var recipe = NewRecipe();
        recipe.AddIngredient("Thịt bò", 500, "gram");

        Assert.Throws<DomainException>(() => recipe.Publish(DateTime.UtcNow));
    }

    [Fact]
    public void Publish_WithStepAndIngredient_SetsPublishedAt()
    {
        var recipe = NewRecipe();
        recipe.AddIngredient("Thịt bò", 500, "gram");
        recipe.AddStep("Nấu", "...");
        var now = new DateTime(2026, 9, 11, 0, 0, 0, DateTimeKind.Utc);

        recipe.Publish(now);

        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.Equal(now, recipe.PublishedAt);
    }

    private static Recipe NewRecipe() =>
        Recipe.Create("Phở bò", "pho-bo", "Mô tả", Guid.NewGuid(), "author-1", 15, 60, 4, RecipeDifficulty.Medium);
}
