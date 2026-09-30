namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Quy tắc nghiệp vụ không có mã riêng trong Phụ lục B và không đáng một lớp riêng (ví dụ: reorder thiếu id bước).
/// Mặc định ánh xạ 400 (CommonExceptionMappings). Lỗi có mã riêng trong Phụ lục B phải dùng lớp con của module.
/// </summary>
public sealed class BusinessRuleViolationException(string code, string message)
    : DomainException(code, message);
