namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// SRS v1.2.2 Phụ lục B — CONCURRENCY_CONFLICT (MT-60): RowVersion không khớp ở một entity mà module của nó
/// không đăng ký bộ dịch lỗi riêng. Mọi BaseEntity đều có concurrency token (§7.1), nên xung đột có thể xảy ra ở
/// mọi module; module Recipe dùng mã riêng RECIPE_CONCURRENCY_CONFLICT.
/// Do UnitOfWork (Infrastructure) sinh ra khi dịch DbUpdateConcurrencyException — handler không tự ném lớp này.
/// </summary>
public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string entityName, Guid? entityId, Exception innerException)
        : base(
            ErrorCodes.ConcurrencyConflict,
            $"{entityName} đã được cập nhật bởi một yêu cầu khác. Hãy tải lại dữ liệu rồi thử lại.",
            innerException)
    {
        EntityName = entityName;
        EntityId = entityId;
        AddExtension("entity", entityName);
        AddExtension("entityId", entityId);
    }

    public string EntityName { get; }

    public Guid? EntityId { get; }
}
