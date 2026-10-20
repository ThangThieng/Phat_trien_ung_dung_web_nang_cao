using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// RECIPE_INVALID_STATE_TRANSITION — chuyển trạng thái nằm ngoài máy trạng thái của SRS §3.3 (MT-35), ví dụ publish
/// một công thức Archived. Extensions mang <c>currentStatus</c> và <c>action</c> để Frontend giải thích cho người dùng.
/// </summary>
public sealed class InvalidRecipeStatusException : RecipeDomainException
{
    public InvalidRecipeStatusException(RecipeStatus from, string action)
        : base(
            ErrorCodes.RecipeInvalidStateTransition,
            $"Không thể thực hiện '{action}' khi công thức đang ở trạng thái {from}.")
    {
        From = from;
        Action = action;
        AddExtension("currentStatus", from.ToString());
        AddExtension("action", action);
    }

    public RecipeStatus From { get; }

    public string Action { get; }
}
