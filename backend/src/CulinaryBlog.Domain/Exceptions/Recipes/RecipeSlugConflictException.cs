namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// RECIPE_SLUG_EXISTS — hai request tranh cùng một slug gần như đồng thời, cùng vượt qua bước kiểm tra chủ động và bị
/// <c>IDX_Recipe_Slug</c> chặn lại (FR-RCP-004 A4). Sinh bởi <c>RecipePersistenceExceptionTranslator</c> từ mã 23505.
/// </summary>
public sealed class RecipeSlugConflictException : RecipeDomainException
{
    public RecipeSlugConflictException(string slug, Exception? innerException = null)
        : base(ErrorCodes.RecipeSlugExists, $"Slug '{slug}' đã được một công thức khác sử dụng. Hãy thử lại.", innerException)
    {
        AddExtension("slug", slug);
    }
}
