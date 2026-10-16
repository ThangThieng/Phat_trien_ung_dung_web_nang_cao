using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cài đặt EF Core của <see cref="IRepository{TEntity}"/>, đăng ký dạng open generic nên mọi entity có sẵn
/// repository cơ bản (ví dụ Dev 2 kiểm tra categoryId bằng IRepository&lt;Category&gt; mà không chờ ICategoryRepository).
/// Repository theo module kế thừa lớp này rồi bổ sung truy vấn đặc thù có tên.
/// Global Query Filter (IsDeleted = false) luôn áp dụng: bản ghi đã xóa mềm coi như không tồn tại.
/// </summary>
public class EfRepository<TEntity>(CulinaryBlogDbContext db) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected CulinaryBlogDbContext Db { get; } = db;

    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Set.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }
}
