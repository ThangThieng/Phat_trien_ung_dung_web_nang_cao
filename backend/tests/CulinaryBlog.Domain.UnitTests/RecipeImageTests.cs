using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.UnitTests;

/// <summary>
/// Buổi 3 — Dev 2: hai quy tắc bắt buộc của module ảnh (SRS §8.4) — chỉ một ảnh chính, và ảnh thay thế khi xóa ảnh chính
/// được chọn theo tiêu chí xác định (OrderIndex nhỏ nhất, hòa thì CreatedAt sớm nhất).
/// </summary>
public class RecipeImageTests
{
    [Fact]
    public void AddImage_FirstIsPrimary_NextAreNot()
    {
        var recipe = NewRecipe();

        var first = recipe.AddImage("http://minio/a.jpg", "Ảnh 1");
        var second = recipe.AddImage("http://minio/b.jpg", null);

        Assert.True(first.IsPrimary);
        Assert.False(second.IsPrimary);
        Assert.Equal([0, 1], recipe.Images.Select(i => i.OrderIndex));
    }

    [Fact]
    public void AddImage_AsPrimary_DemotesCurrentPrimary()
    {
        var recipe = NewRecipe();
        var first = recipe.AddImage("http://minio/a.jpg", null);

        var second = recipe.AddImage("http://minio/b.jpg", null, isPrimary: true);

        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
        Assert.Single(recipe.Images, i => i.IsPrimary);
    }

    [Fact]
    public void SetPrimaryImage_KeepsExactlyOnePrimary()
    {
        var recipe = NewRecipe();
        recipe.AddImage("http://minio/a.jpg", null);
        var second = recipe.AddImage("http://minio/b.jpg", null);
        recipe.AddImage("http://minio/c.jpg", null);

        recipe.SetPrimaryImage(second.Id);

        Assert.Equal(second.Id, Assert.Single(recipe.Images, i => i.IsPrimary).Id);
    }

    [Fact]
    public void UpdateImageMetadata_OnlyChangesSentFields()
    {
        var recipe = NewRecipe();
        var image = recipe.AddImage("http://minio/a.jpg", "Phở bò");

        recipe.UpdateImageMetadata(image.Id, altText: null, orderIndex: 5);

        Assert.Equal("Phở bò", image.AltText);
        Assert.Equal(5, image.OrderIndex);
    }

    /// <summary>Xóa ảnh chính → xóa MỀM, rồi ảnh OrderIndex nhỏ nhất lên thay (không phụ thuộc thứ tự trả về của DB).</summary>
    [Fact]
    public void RemovePrimary_SoftDeletes_ThenLowestOrderIndexIsPromoted()
    {
        var recipe = NewRecipe();
        var primary = recipe.AddImage("http://minio/a.jpg", null);
        var later = recipe.AddImage("http://minio/b.jpg", null, orderIndex: 7);
        var earlier = recipe.AddImage("http://minio/c.jpg", null, orderIndex: 2);

        var removed = recipe.RemoveImage(primary.Id);
        var promoted = recipe.PromoteFallbackPrimaryImage();

        Assert.True(removed.IsDeleted);
        Assert.False(removed.IsPrimary);
        Assert.DoesNotContain(recipe.Images, i => i.Id == primary.Id);
        Assert.Equal(earlier.Id, promoted?.Id);
        Assert.False(later.IsPrimary);
    }

    [Fact]
    public void RemoveNonPrimary_DoesNotChangePrimary()
    {
        var recipe = NewRecipe();
        var primary = recipe.AddImage("http://minio/a.jpg", null);
        var other = recipe.AddImage("http://minio/b.jpg", null);

        recipe.RemoveImage(other.Id);

        Assert.Null(recipe.PromoteFallbackPrimaryImage());
        Assert.True(primary.IsPrimary);
    }

    [Fact]
    public void UnknownImage_ThrowsRecipeImageNotFound()
    {
        var recipe = NewRecipe();

        Assert.Throws<RecipeImageNotFoundException>(() => recipe.SetPrimaryImage(Guid.NewGuid()));
        Assert.Throws<RecipeImageNotFoundException>(() => recipe.RemoveImage(Guid.NewGuid()));
    }

    private static Recipe NewRecipe() =>
        Recipe.Create("Phở bò Hà Nội", "pho-bo-ha-noi", "Phở bò nước dùng trong.", Guid.NewGuid(), "author-1", 30, 180, 4, RecipeDifficulty.Hard);
}
