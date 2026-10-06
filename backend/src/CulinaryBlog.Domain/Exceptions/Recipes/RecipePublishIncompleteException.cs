namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// RECIPE_PUBLISH_INCOMPLETE — xuất bản công thức chưa có đủ ít nhất 1 bước VÀ 1 nguyên liệu (FR-RCP-005 A1, MT-06).
/// Thông điệp và extension <c>missing</c> chỉ rõ thiếu phần nào để Frontend dẫn người dùng tới đúng bước của trình soạn.
/// Thay cho BusinessRuleViolationException tạm dùng từ Buổi 2 (retrofit D-15, Buổi 4).
/// </summary>
public sealed class RecipePublishIncompleteException : RecipeDomainException
{
    public RecipePublishIncompleteException(bool missingSteps, bool missingIngredients)
        : base(ErrorCodes.RecipePublishIncomplete, BuildMessage(missingSteps, missingIngredients))
    {
        string[] missing = [.. new[] { missingSteps ? "steps" : null, missingIngredients ? "ingredients" : null }.OfType<string>()];
        Missing = missing;
        AddExtension("missing", missing);
    }

    public IReadOnlyList<string> Missing { get; }

    private static string BuildMessage(bool missingSteps, bool missingIngredients) => (missingSteps, missingIngredients) switch
    {
        (true, true) => "Không thể xuất bản: công thức chưa có bước thực hiện và chưa có nguyên liệu nào.",
        (true, false) => "Không thể xuất bản: công thức chưa có bước thực hiện nào.",
        _ => "Không thể xuất bản: công thức chưa có nguyên liệu nào.",
    };
}
