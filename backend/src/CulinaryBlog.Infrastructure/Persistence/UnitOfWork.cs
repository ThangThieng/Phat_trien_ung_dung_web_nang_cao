using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Cài đặt <see cref="IUnitOfWork"/> trên <see cref="CulinaryBlogDbContext"/> (Buổi 3 — Dev 4).
/// Hai trách nhiệm ngoài SaveChanges:
/// 1. Dịch lỗi ghi DB sang domain exception — handler không bắt được exception của EF vì Application không tham chiếu EF.
/// 2. Transaction tường minh qua execution strategy — DbContext bật EnableRetryOnFailure nên EF ném
///    InvalidOperationException nếu code tự BeginTransaction bên ngoài strategy.
/// Property repository theo module (Users, Categories, Recipes) do từng dev thêm vào (append-only).
/// </summary>
public sealed class UnitOfWork(
    CulinaryBlogDbContext db,
    IEnumerable<IPersistenceExceptionTranslator> translators) : IUnitOfWork
{
    private readonly IPersistenceExceptionTranslator[] _translators = translators.ToArray();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            throw Translate(ex);
        }
    }

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(
            async ct =>
            {
                // Mỗi lần thử lại bắt đầu từ trạng thái sạch — thay đổi của lần thử trước đã bị rollback cùng transaction.
                db.ChangeTracker.Clear();

                var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
                await using (transaction.ConfigureAwait(false))
                {
                    await operation(ct).ConfigureAwait(false);

                    // Lỗi xảy ra trước dòng này → DisposeAsync của transaction tự rollback.
                    await transaction.CommitAsync(ct).ConfigureAwait(false);
                }
            },
            cancellationToken);
    }

    /// <summary>
    /// Thứ tự: bộ dịch của module (mã lỗi nghiệp vụ riêng, ví dụ RECIPE_CONCURRENCY_CONFLICT, CATEGORY_NAME_EXISTS)
    /// → xung đột RowVersion không module nào nhận → CONCURRENCY_CONFLICT (409) → còn lại giữ nguyên để
    /// GlobalExceptionMiddleware trả 500 INTERNAL_ERROR (lỗi không lường trước, cần log đầy đủ).
    /// </summary>
    private Exception Translate(DbUpdateException exception)
    {
        foreach (var translator in _translators)
        {
            if (translator.TryTranslate(exception) is { } domainError)
            {
                return domainError;
            }
        }

        if (exception is DbUpdateConcurrencyException concurrency)
        {
            var entry = concurrency.Entries.Count > 0 ? concurrency.Entries[0] : null;
            return new ConcurrencyConflictException(
                entry?.Metadata.ClrType.Name ?? "Bản ghi",
                (entry?.Entity as BaseEntity)?.Id,
                concurrency);
        }

        return exception;
    }
}
