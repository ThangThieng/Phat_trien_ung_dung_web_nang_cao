using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository ghi dữ liệu (phía Command của CQRS) cho một entity kế thừa <see cref="BaseEntity"/>.
/// Trả entity được EF theo dõi: đổi dữ liệu qua phương thức Domain rồi gọi <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// Cố ý nhỏ:
/// • KHÔNG có Remove/Delete — mọi xóa là xóa mềm qua phương thức Domain (NFR-REL-003); xóa vật lý chỉ ở PermanentPurgeJob.
/// • KHÔNG trả IQueryable — không để chi tiết EF (Include, IgnoreQueryFilters) lọt lên Application; truy vấn đặc thù là
///   phương thức có tên trên repository của module.
/// • KHÔNG có Update — entity đã được theo dõi; Update(entity) sẽ đánh dấu mọi cột là đã sửa.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
}
