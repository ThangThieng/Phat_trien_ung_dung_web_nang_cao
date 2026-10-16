namespace CulinaryBlog.Domain.Exceptions.Categories;

/// <summary>
/// Lớp gốc của mọi lỗi nghiệp vụ module Category (SRS §3.2, Phụ lục B). Trừu tượng để mỗi tình huống có một lớp
/// riêng với đúng một mã lỗi; Domain không mang mã HTTP — <c>CategoryExceptionMappings</c> ở tầng API gán mã.
/// </summary>
public abstract class CategoryDomainException(string code, string message, Exception? innerException = null)
    : DomainException(code, message, innerException);
