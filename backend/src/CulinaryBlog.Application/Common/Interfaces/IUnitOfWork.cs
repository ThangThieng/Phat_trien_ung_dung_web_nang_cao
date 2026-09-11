namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Unit of Work – mọi thay đổi được commit trong một lần SaveChanges (một transaction).</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
