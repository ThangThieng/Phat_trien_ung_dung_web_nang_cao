namespace CulinaryBlog.Domain.Exceptions.Auth;

/// <summary>
/// Lớp gốc của mọi lỗi nghiệp vụ module Auth (SRS §3.1, Phụ lục B). Trừu tượng để mỗi tình huống có một lớp riêng;
/// Domain không mang mã HTTP — <c>AuthExceptionMappings</c> ở tầng API gán mã.
/// </summary>
public abstract class AuthDomainException(string code, string message, Exception? innerException = null)
    : DomainException(code, message, innerException);
