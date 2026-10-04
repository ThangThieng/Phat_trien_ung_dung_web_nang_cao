using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Categories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// FR-CAT-003 A4 / SRS v1.2.2 MT-63 – lớp phòng vệ THỨ HAI cho tên danh mục trùng: hai Admin tạo hoặc đổi cùng một tên
/// gần như đồng thời có thể cùng vượt qua bước kiểm tra chủ động của handler, chỉ ràng buộc UNIQUE của PostgreSQL chặn lại.
/// Mã <c>23505</c> trên <c>IDX_Category_Name</c> được dịch thành <see cref="CategoryNameAlreadyExistsException"/> (409),
/// không để lọt thành 500. Chi tiết của PostgreSQL chỉ được đọc ở Infrastructure — tầng API chỉ thấy exception nghiệp vụ.
/// Được <c>UnitOfWork</c> gọi; đăng ký tự động (DI quét mọi <see cref="IPersistenceExceptionTranslator"/> trong assembly).
/// </summary>
internal sealed class CategoryPersistenceExceptionTranslator : IPersistenceExceptionTranslator
{
    /// <summary>Tên index khai báo ở <c>CategoryConfiguration</c> (SRS §7.6).</summary>
    internal const string NameIndexName = "IDX_Category_Name";

    public DomainException? TryTranslate(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres
            || postgres.ConstraintName != NameIndexName)
        {
            return null;
        }

        var name = exception.Entries
            .Select(entry => entry.Entity)
            .OfType<Category>()
            .Select(category => category.Name)
            .FirstOrDefault();

        return new CategoryNameAlreadyExistsException(name ?? "(không rõ tên)", exception);
    }
}
