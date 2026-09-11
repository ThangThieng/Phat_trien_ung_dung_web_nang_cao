namespace CulinaryBlog.Domain.Common;

/// <summary>SRS §7.1 – mọi entity kế thừa: Id, CreatedAt, UpdatedAt, IsDeleted (soft delete), RowVersion (optimistic concurrency).</summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
