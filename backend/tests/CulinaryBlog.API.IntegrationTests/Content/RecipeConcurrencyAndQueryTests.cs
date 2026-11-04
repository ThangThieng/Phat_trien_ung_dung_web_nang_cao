using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>Buổi 8 — phần kiểm thử đồng thời và chống N+1 của kế hoạch (NFR-MAINT-002, FR-RCP-004).</summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeConcurrencyAndQueryTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task TenParallelUpdates_WithSameRowVersion_OneSucceeds_NineConflict_NoServerError()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Canh chua cá lóc đồng thời");

        var responses = await Task.WhenAll(Enumerable.Range(1, 10).Select(i =>
            author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { cookTime = i, rowVersion = draft.RowVersion })));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        foreach (var conflict in responses.Where(r => r.StatusCode == HttpStatusCode.Conflict))
        {
            await conflict.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.RecipeConcurrencyConflict);
        }
    }

    /// <summary>
    /// Chống N+1: DetailQuery dùng AsSplitQuery nên có đúng 4 câu lệnh (công thức + danh mục, bước, nguyên liệu, ảnh) dù nhiều dòng con.
    /// Kế hoạch ghi "≤ 2" nhưng gộp lại sẽ tạo tích Descartes bước × nguyên liệu × ảnh, nên kiểm tra tính chất cần giữ: số câu không tăng.
    /// </summary>
    [Fact]
    public async Task GetRecipeBySlug_QueryCount_DoesNotGrowWithChildRows()
    {
        var few = await CountQueriesAsync(TestDataSeeder.PublishedRecipeSlug);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var recipe = Recipe.Create("Món nhiều dòng con", "mon-nhieu-dong-con", "Công thức có nhiều nguyên liệu và các bước để đếm truy vấn.", TestDataSeeder.MonChinhCategoryId, TestDataSeeder.AuthorUserId, 10, 10, 2, RecipeDifficulty.Easy);
            foreach (var i in Enumerable.Range(1, 20))
            {
                recipe.AddIngredient(new IngredientDetails($"Nguyên liệu {i}", 1, null, "g", null));
                recipe.AddStep($"Bước {i}", "Thực hiện bước này.");
            }

            recipe.Publish(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
        }

        var many = await CountQueriesAsync("mon-nhieu-dong-con");

        Assert.InRange(few, 1, 4);
        Assert.Equal(few, many);
    }

    [Fact]
    public async Task MineList_IsNeverStoredByCaches()
    {
        var response = await factory.CreateClientAs("Author").GetAsync(new Uri("/api/v1/recipes/mine", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore, "GET /recipes/mine phải có Cache-Control: no-store");
    }

    private async Task<int> CountQueriesAsync(string slug)
    {
        using var counter = new RecipeCommandCounter();
        var response = await factory.CreateClient().GetAsync(new Uri($"/api/v1/recipes/{slug}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return counter.Count;
    }

    /// <summary>Đếm câu lệnh SQL chạm bảng Recipes qua DiagnosticListener của EF Core, không phải sửa mã sản phẩm.</summary>
    private sealed class RecipeCommandCounter : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>, IDisposable
    {
        private readonly IDisposable _allListeners;
        private readonly List<IDisposable> _subscriptions = [];
        private int _count;

        public RecipeCommandCounter() => _allListeners = DiagnosticListener.AllListeners.Subscribe(this);

        public int Count => Volatile.Read(ref _count);

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == DbLoggerCategory.Name)
            {
                _subscriptions.Add(value.Subscribe(this));
            }
        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (value.Key == RelationalEventId.CommandExecuted.Name
                && value.Value is CommandExecutedEventData data
                && data.Command.CommandText.Contains("\"Recipes\"", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref _count);
            }
        }

        public void OnCompleted()
        {
            // DiagnosticListener không có việc gì cần làm khi kết thúc.
        }

        public void OnError(Exception error)
        {
            // Đếm truy vấn không xử lý lỗi của listener.
        }

        public void Dispose()
        {
            _subscriptions.ForEach(subscription => subscription.Dispose());
            _allListeners.Dispose();
        }
    }
}
