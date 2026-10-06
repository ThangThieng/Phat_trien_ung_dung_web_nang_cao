using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.UnitTests;

/// <summary>Buổi 4 — Dev 2: bất biến nội dung công thức — FR-RCP-009 (định lượng hai cột), FR-RCP-010 (đánh số bước), FR-RCP-004 (khóa slug).</summary>
public class RecipeContentTests
{
    [Fact]
    public void RemoveMiddleStep_RenumbersRemainingSteps()
    {
        var recipe = RecipeWithSteps(4);
        var second = recipe.Steps.Single(s => s.StepNumber == 2);

        recipe.RemoveStep(second.Id);

        Assert.Equal([1, 2, 3], recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.StepNumber));
        Assert.Equal(["Bước 1", "Bước 3", "Bước 4"], recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.Title));
    }

    [Fact]
    public void ReorderSteps_AssignsNumbersInGivenOrder()
    {
        var recipe = RecipeWithSteps(3);
        var ids = recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.Id).ToArray();

        recipe.ReorderSteps([ids[2], ids[0], ids[1]]);

        Assert.Equal(["Bước 3", "Bước 1", "Bước 2"], recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.Title));
    }

    [Fact]
    public void ReorderSteps_WithMissingId_Throws400AndKeepsOrder()
    {
        var recipe = RecipeWithSteps(3);
        var ids = recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.Id).ToArray();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => recipe.ReorderSteps([ids[1], ids[0]]));

        Assert.Equal(ErrorCodes.ValidationError, ex.Code);
        Assert.Contains(ids[2].ToString(), ex.Message, StringComparison.Ordinal);
        Assert.Equal(["Bước 1", "Bước 2", "Bước 3"], recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.Title));
    }

    [Fact]
    public void ReorderSteps_WithDuplicateOrForeignId_Throws()
    {
        var recipe = RecipeWithSteps(2);
        var ids = recipe.Steps.Select(s => s.Id).ToArray();

        Assert.Throws<BusinessRuleViolationException>(() => recipe.ReorderSteps([ids[0], ids[0]]));
        Assert.Throws<BusinessRuleViolationException>(() => recipe.ReorderSteps([ids[0], ids[1], Guid.NewGuid()]));
    }

    [Fact]
    public void AddIngredient_WithoutQuantityQuantityTextAndUnit_ThrowsQuantityRequired()
    {
        var recipe = NewRecipe();

        var ex = Assert.Throws<IngredientQuantityRequiredException>(() =>
            recipe.AddIngredient(new IngredientDetails("Muối", null, "   ", null, "nêm")));

        Assert.Equal(ErrorCodes.IngredientQuantityRequired, ex.Code);
        Assert.Empty(recipe.Ingredients);
    }

    [Theory]
    [InlineData(1.5, null, null)]
    [InlineData(null, "vừa đủ", null)]
    [InlineData(null, null, "củ")]
    public void AddIngredient_WithAnyOneOfTheThree_IsAccepted(double? quantity, string? quantityText, string? unit)
    {
        var recipe = NewRecipe();

        var ingredient = recipe.AddIngredient(new IngredientDetails(" Hành tím ", (decimal?)quantity, quantityText, unit, null));

        Assert.Equal("Hành tím", ingredient.Name);
        Assert.Equal(quantityText, ingredient.QuantityText);
    }

    [Fact]
    public void UpdateIngredient_CanSwitchFromNumberToText()
    {
        var recipe = NewRecipe();
        var ingredient = recipe.AddIngredient(new IngredientDetails("Đường", 20, null, "gram", null));

        recipe.UpdateIngredient(ingredient.Id, new IngredientDetails("Đường", null, "vừa ăn", null, null), orderIndex: null);

        Assert.Null(ingredient.Quantity);
        Assert.Null(ingredient.Unit);
        Assert.Equal("vừa ăn", ingredient.QuantityText);
    }

    [Fact]
    public void IsSlugLocked_OnlyFalseForNeverPublishedDraft()
    {
        var draft = RecipeWithSteps(1);
        draft.AddIngredient(new IngredientDetails("Thịt bò", 500, null, "gram", null));
        Assert.False(draft.IsSlugLocked);

        draft.Publish(DateTime.UtcNow);
        Assert.True(draft.IsSlugLocked);

        draft.Unpublish();
        Assert.True(draft.IsSlugLocked); // đã từng xuất bản → khóa vĩnh viễn (MT-27)
        Assert.Throws<InvalidOperationException>(() => draft.ChangeSlug("slug-moi"));
    }

    private static Recipe RecipeWithSteps(int count)
    {
        var recipe = NewRecipe();
        for (var i = 1; i <= count; i++)
        {
            recipe.AddStep($"Bước {i}", $"Mô tả bước {i}.");
        }

        return recipe;
    }

    private static Recipe NewRecipe() =>
        Recipe.Create("Bún bò Huế", "bun-bo-hue", "Bún bò Huế cay nồng mùi sả.", Guid.NewGuid(), "author-1", 30, 120, 4, RecipeDifficulty.Medium);
}
