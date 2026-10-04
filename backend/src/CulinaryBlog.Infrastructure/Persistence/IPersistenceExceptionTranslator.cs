using CulinaryBlog.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Dịch lỗi ghi DB (DbUpdateException/DbUpdateConcurrencyException, mã PostgreSQL như 23505) thành domain exception
/// của MỘT module. Đây là chỗ DUY NHẤT trong hệ thống được đọc chi tiết của PostgreSQL (SqlState, ConstraintName) —
/// nhờ vậy tầng API không tham chiếu Npgsql (SRS v1.2.2 MT-63).
/// Mỗi module một cài đặt (ví dụ RecipePersistenceExceptionTranslator của Dev 2, CategoryPersistenceExceptionTranslator
/// của Dev 3); mọi cài đặt trong assembly Infrastructure được đăng ký tự động nên dev module không sửa file DI.
/// </summary>
public interface IPersistenceExceptionTranslator
{
    /// <summary>Trả domain exception tương ứng nếu lỗi thuộc module này; ngược lại trả <c>null</c> để bộ dịch khác xử lý.</summary>
    DomainException? TryTranslate(DbUpdateException exception);
}
