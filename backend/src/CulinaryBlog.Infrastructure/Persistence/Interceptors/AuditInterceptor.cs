using CulinaryBlog.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CulinaryBlog.Infrastructure.Persistence.Interceptors;

/// <summary>
/// SRS §6.2 – tự set CreatedAt/UpdatedAt và sinh RowVersion mới mỗi lần lưu.
/// PostgreSQL không tự sinh rowversion như SQL Server, nên RowVersion (bytea, concurrency token) được cấp tại đây:
/// UPDATE ... WHERE "RowVersion" = @original → lệch là DbUpdateConcurrencyException (FR-RCP-004).
/// </summary>
public sealed class AuditInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                    break;
                default:
                    break;
            }
        }
    }
}
