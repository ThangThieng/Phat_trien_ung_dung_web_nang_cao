using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.Domain.UnitTests;

/// <summary>
/// Buổi 4 — Dev 4: máy trạng thái Recipe (FR-RCP-005/006, MT-35, retrofit D-15). Kiểm thử ĐỦ 12 tổ hợp
/// (3 trạng thái × 4 hành động) thay vì chỉ đường hạnh phúc: lỗi của v1.0.0 không phải "code sai" mà là "có một chuyển
/// đổi không tồn tại và không ai nhận ra" (Archived vào được, không ra được) — chỉ liệt kê toàn bộ ma trận mới lộ ra.
/// </summary>
public class RecipeLifecycleTests
{
    private static readonly DateTime FirstPublish = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>5 tổ hợp hợp lệ → đúng trạng thái đích.</summary>
    [Theory]
    [InlineData(RecipeStatus.Draft, RecipeAction.Publish, RecipeStatus.Published)]
    [InlineData(RecipeStatus.Published, RecipeAction.Unpublish, RecipeStatus.Draft)]
    [InlineData(RecipeStatus.Draft, RecipeAction.Archive, RecipeStatus.Archived)]
    [InlineData(RecipeStatus.Published, RecipeAction.Archive, RecipeStatus.Archived)]
    [InlineData(RecipeStatus.Archived, RecipeAction.Unarchive, RecipeStatus.Draft)]
    public void ValidTransition_MovesToTargetStatus(RecipeStatus from, RecipeAction action, RecipeStatus expected)
    {
        var recipe = RecipeIn(from);

        Apply(recipe, action);

        Assert.Equal(expected, recipe.Status);
    }

    /// <summary>7 tổ hợp còn lại → InvalidRecipeStatusException (409), trạng thái không đổi.</summary>
    [Theory]
    [InlineData(RecipeStatus.Draft, RecipeAction.Unpublish)]
    [InlineData(RecipeStatus.Draft, RecipeAction.Unarchive)]
    [InlineData(RecipeStatus.Published, RecipeAction.Publish)]
    [InlineData(RecipeStatus.Published, RecipeAction.Unarchive)]
    [InlineData(RecipeStatus.Archived, RecipeAction.Publish)]
    [InlineData(RecipeStatus.Archived, RecipeAction.Unpublish)]
    [InlineData(RecipeStatus.Archived, RecipeAction.Archive)]
    public void InvalidTransition_ThrowsInvalidRecipeStatusException(RecipeStatus from, RecipeAction action)
    {
        var recipe = RecipeIn(from);

        var ex = Assert.Throws<InvalidRecipeStatusException>(() => Apply(recipe, action));

        Assert.Equal(ErrorCodes.RecipeInvalidStateTransition, ex.Code);
        Assert.Equal(from, ex.From);
        Assert.Equal(action.ToString(), ex.Action);
        Assert.Equal(from, recipe.Status);
    }

    /// <summary>Bảng chuyển trạng thái là nguồn duy nhất: đúng 5 dòng, hai Theory ở trên phủ đủ 12 tổ hợp.</summary>
    [Fact]
    public void TransitionTable_HasExactlyFiveValidTransitions()
    {
        Assert.Equal(5, RecipeStatusTransitions.All.Count);
        Assert.Equal(12, Enum.GetValues<RecipeStatus>().Length * Enum.GetValues<RecipeAction>().Length);
    }

    [Fact]
    public void Republish_KeepsFirstPublishedAt()
    {
        var recipe = RecipeIn(RecipeStatus.Published);

        recipe.Unpublish();
        recipe.Publish(FirstPublish.AddDays(10));

        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.Equal(FirstPublish, recipe.PublishedAt);
    }

    [Fact]
    public void ArchiveThenUnarchive_KeepsPublishedAt()
    {
        var recipe = RecipeIn(RecipeStatus.Published);

        recipe.Archive();
        recipe.Unarchive();

        Assert.Equal(RecipeStatus.Draft, recipe.Status);
        Assert.Equal(FirstPublish, recipe.PublishedAt);
    }

    /// <summary>Archived chưa đủ nội dung vẫn là 409 (chuyển trạng thái được kiểm tra TRƯỚC điều kiện nội dung).</summary>
    [Fact]
    public void PublishArchivedRecipe_ReportsTransitionBeforeCompleteness()
    {
        var recipe = NewRecipe(withContent: false);
        recipe.Archive();

        Assert.Throws<InvalidRecipeStatusException>(() => recipe.Publish(FirstPublish));
    }

    [Theory]
    [InlineData(false, false, new[] { "steps", "ingredients" })]
    [InlineData(true, false, new[] { "steps" })]
    [InlineData(false, true, new[] { "ingredients" })]
    public void PublishIncomplete_ReportsWhatIsMissing(bool withIngredient, bool withStep, string[] missing)
    {
        var recipe = NewRecipe(withContent: false);
        if (withIngredient)
        {
            recipe.AddIngredient("Thịt bò", 500, "gram");
        }

        if (withStep)
        {
            recipe.AddStep("Nấu", "Nấu chín thịt bò.");
        }

        var ex = Assert.Throws<RecipePublishIncompleteException>(() => recipe.Publish(FirstPublish));

        Assert.Equal(missing, ex.Missing);
        Assert.Equal(RecipeStatus.Draft, recipe.Status);
        Assert.Null(recipe.PublishedAt);
    }

    /// <summary>FR-RCP-007: xóa mềm chỉ gán IsDeleted — trạng thái, bước, nguyên liệu, ảnh giữ nguyên (khôi phục được).</summary>
    [Fact]
    public void SoftDelete_OnlyFlagsTheRecipe()
    {
        var recipe = RecipeIn(RecipeStatus.Published);
        recipe.AddImage("http://minio/culinary-blog/recipes/a.jpg", "Ảnh");

        recipe.SoftDelete();

        Assert.True(recipe.IsDeleted);
        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.Single(recipe.Steps);
        Assert.Single(recipe.Ingredients);
        Assert.Single(recipe.Images);
        Assert.All(recipe.Steps, s => Assert.False(s.IsDeleted));
    }

    private static Recipe RecipeIn(RecipeStatus status)
    {
        var recipe = NewRecipe(withContent: true);
        switch (status)
        {
            case RecipeStatus.Published:
                recipe.Publish(FirstPublish);
                break;
            case RecipeStatus.Archived:
                recipe.Archive();
                break;
            default:
                break;
        }

        Assert.Equal(status, recipe.Status);
        return recipe;
    }

    private static void Apply(Recipe recipe, RecipeAction action)
    {
        switch (action)
        {
            case RecipeAction.Publish:
                recipe.Publish(FirstPublish.AddDays(1));
                break;
            case RecipeAction.Unpublish:
                recipe.Unpublish();
                break;
            case RecipeAction.Archive:
                recipe.Archive();
                break;
            case RecipeAction.Unarchive:
                recipe.Unarchive();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    private static Recipe NewRecipe(bool withContent)
    {
        var recipe = Recipe.Create("Phở bò", "pho-bo", "Phở bò Hà Nội nước dùng trong.", Guid.NewGuid(), "author-1", 30, 180, 4, RecipeDifficulty.Hard);
        if (withContent)
        {
            recipe.AddIngredient("Thịt bò", 500, "gram");
            recipe.AddStep("Ninh xương", "Ninh xương bò trong 6 giờ.");
        }

        return recipe;
    }
}
