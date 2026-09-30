namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

/// <summary>
/// Unit of Work — mọi repository lấy từ cùng một IUnitOfWork dùng chung một DbContext, nên một lần
/// <see cref="SaveChangesAsync"/> lưu tất cả hoặc không lưu gì (SRS §3.3: mọi mutation đi qua UnitOfWork).
/// Repository theo module được mỗi dev thêm vào đây bằng MỘT property (append-only), ví dụ:
/// <c>IUserRepository Users { get; }</c> (Dev 1), <c>ICategoryRepository Categories { get; }</c> (Dev 3),
/// <c>IRecipeRepository Recipes { get; }</c> (Dev 2).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Repository của module Auth (người dùng + refresh token) — Buổi 3, Dev 1.</summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Lưu mọi thay đổi trong một transaction. Lỗi ghi DB đã được dịch sang domain exception
    /// (ConcurrencyConflictException hoặc lỗi của module) — handler KHÔNG cần và KHÔNG được bắt exception của EF.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Chạy nhiều lần SaveChanges trong MỘT transaction tường minh (commit khi <paramref name="operation"/> xong,
    /// rollback khi nó ném lỗi). ⚠️ <paramref name="operation"/> có thể bị chạy lại (execution strategy retry khi mất
    /// kết nối) nên phải tự đọc dữ liệu BÊN TRONG nó — không dùng entity đã nạp từ trước khi gọi phương thức này.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
