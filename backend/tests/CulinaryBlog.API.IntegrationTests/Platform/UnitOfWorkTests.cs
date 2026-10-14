using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// Buổi 3 — Dev 4: Unit of Work trên PostgreSQL THẬT. Hai hành vi chỉ chứng minh được trên DB thật (In-Memory
/// không có transaction lẫn concurrency token thật): transaction commit/rollback đi qua execution strategy, và
/// xung đột RowVersion được dịch thành domain exception thay vì lọt ra thành 500.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class UnitOfWorkTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ExecuteInTransaction_Commits_WhenOperationSucceeds()
    {
        var category = NewCategory();

        using (var scope = factory.Services.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var categories = scope.ServiceProvider.GetRequiredService<IRepository<Category>>();

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await categories.AddAsync(category, ct);
                await unitOfWork.SaveChangesAsync(ct);
            });
        }

        Assert.True(await ExistsAsync(category.Id));
    }

    [Fact]
    public async Task ExecuteInTransaction_RollsBackEverySave_WhenOperationThrows()
    {
        var first = NewCategory();
        var second = NewCategory();

        using (var scope = factory.Services.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var categories = scope.ServiceProvider.GetRequiredService<IRepository<Category>>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await categories.AddAsync(first, ct);
                await unitOfWork.SaveChangesAsync(ct);   // lần lưu 1 đã xuống DB (trong transaction)
                await categories.AddAsync(second, ct);
                await unitOfWork.SaveChangesAsync(ct);   // lần lưu 2
                throw new InvalidOperationException("Lỗi giữa chừng — cả hai lần lưu phải bị hoàn tác.");
            }));
        }

        Assert.False(await ExistsAsync(first.Id));
        Assert.False(await ExistsAsync(second.Id));
    }

    [Fact]
    public async Task StaleRowVersion_IsTranslatedToConcurrencyConflict()
    {
        using var firstScope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();

        var firstCopy = await firstScope.ServiceProvider.GetRequiredService<IRepository<Category>>().GetByIdAsync(TestDataSeeder.MonChinhCategoryId);
        var secondCopy = await secondScope.ServiceProvider.GetRequiredService<IRepository<Category>>().GetByIdAsync(TestDataSeeder.MonChinhCategoryId);
        Assert.NotNull(firstCopy);
        Assert.NotNull(secondCopy);

        // Hai Admin cùng mở một danh mục; người thứ nhất lưu trước → RowVersion trong DB đổi.
        firstCopy.UpdatedAt = DateTime.UtcNow;
        await firstScope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

        secondCopy.UpdatedAt = DateTime.UtcNow;
        var conflict = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => secondScope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync());

        Assert.Equal(ErrorCodes.ConcurrencyConflict, conflict.Code);
        Assert.Equal(nameof(Category), conflict.EntityName);
        Assert.Equal(TestDataSeeder.MonChinhCategoryId, conflict.EntityId);
    }

    private static Category NewCategory()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return Category.Create($"Danh mục UoW {suffix}", $"danh-muc-uow-{suffix}");
    }

    private async Task<bool> ExistsAsync(Guid id)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IRepository<Category>>().ExistsAsync(id);
    }
}
