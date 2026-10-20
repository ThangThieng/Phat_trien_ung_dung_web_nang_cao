namespace CulinaryBlog.Domain.Exceptions.Recipes;

/// <summary>
/// Lớp gốc của mọi lỗi nghiệp vụ module Recipe (SRS §3.3, Phụ lục B). Trừu tượng để mỗi tình huống có một lớp riêng;
/// Domain không mang mã HTTP — <c>RecipeExceptionMappings</c> ở tầng API gán mã.
/// </summary>
public abstract class RecipeDomainException(string code, string message, Exception? innerException = null)
    : DomainException(code, message, innerException);
