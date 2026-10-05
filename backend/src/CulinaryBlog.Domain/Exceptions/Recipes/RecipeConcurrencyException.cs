namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// RECIPE_CONCURRENCY_CONFLICT — RowVersion không khớp khi ghi một thành phần của aggregate Recipe (FR-RCP-004 A2, MT-09).
/// Không ai ném tay: <c>RecipePersistenceExceptionTranslator</c> sinh ra từ DbUpdateConcurrencyException, nên handler
/// không bao giờ phải bắt exception của EF.
/// </summary>
public sealed class RecipeConcurrencyException : RecipeDomainException
{
    public RecipeConcurrencyException(Guid? recipeId, Exception innerException)
        : base(
            ErrorCodes.RecipeConcurrencyConflict,
            "Công thức đã bị thay đổi bởi người dùng khác, vui lòng tải lại.",
            innerException)
    {
        RecipeId = recipeId;
        AddExtension("recipeId", recipeId);
    }

    public Guid? RecipeId { get; }
}
