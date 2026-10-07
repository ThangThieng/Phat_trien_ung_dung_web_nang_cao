using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>One database coordination point for all token operations affecting the same user.</summary>
internal static class UserTokenTransaction
{
    public static async Task<T> ExecuteAsync<T>(CulinaryBlogDbContext db, string userId, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is not null)
        {
            await LockUserAsync(db, userId, cancellationToken).ConfigureAwait(false);
            return await operation(cancellationToken).ConfigureAwait(false);
        }

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await LockUserAsync(db, userId, cancellationToken).ConfigureAwait(false);
                var result = await operation(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch
            {
                // An execution-strategy retry must reload identities/tokens after rollback.
                db.ChangeTracker.Clear();
                throw;
            }
        }).ConfigureAwait(false);
    }

    private static Task<int> LockUserAsync(CulinaryBlogDbContext db, string userId, CancellationToken cancellationToken) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT \"Id\" FROM \"AspNetUsers\" WHERE \"Id\" = {userId} FOR UPDATE", cancellationToken);
}
