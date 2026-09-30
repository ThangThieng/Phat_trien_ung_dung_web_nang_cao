using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Trả nợ kiểm thử Buổi 3 của Dev 2 (bổ sung ở Buổi 4): FR-RCP-003 tạo nháp, FR-RCP-008 ảnh (đổi ảnh chính trong một
/// transaction hai bước, xóa mềm + xếp hàng xóa tệp) và bộ dịch lỗi RowVersion của aggregate Recipe trên PostgreSQL thật.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeDraftAndImageEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateDraft_Returns201AsDraftWithoutPublishedAt()
    {
        var draft = await RecipeApi.CreateDraftAsync(factory.CreateClientAs("Author"), "Gà nướng mật ong");

        Assert.Equal(Domain.Enums.RecipeStatus.Draft, draft.Status);
        Assert.Null(draft.PublishedAt);
        Assert.Equal("ga-nuong-mat-ong", draft.Slug);
        Assert.Equal("Món chính", draft.Category.Name);
        Assert.Equal("IT Author", draft.Author.DisplayName);
    }

    [Fact]
    public async Task CreateDraft_InvalidBody400_UnknownCategory400OnField_Guest401()
    {
        var author = factory.CreateClientAs("Author");

        var invalid = await author.PostAsJsonAsync("/api/v1/recipes", new { title = "Ab", description = "ngắn", categoryId = TestDataSeeder.MonChinhCategoryId, prepTime = 0, cookTime = 10, servings = 1, difficulty = "Easy" });
        await invalid.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        var errors = await invalid.ReadValidationErrorsAsync();
        Assert.Contains("title", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("description", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("prepTime", errors.Keys, StringComparer.Ordinal);

        var unknownCategory = await author.PostAsJsonAsync("/api/v1/recipes", new { title = "Lẩu thái chua cay", description = "Lẩu thái chua cay hải sản cho cả nhà.", categoryId = Guid.NewGuid(), prepTime = 20, cookTime = 30, servings = 4, difficulty = "Medium" });
        await unknownCategory.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("categoryId", (await unknownCategory.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);

        var guest = await factory.CreateClient().PostAsJsonAsync("/api/v1/recipes", new { title = "Lẩu thái chua cay" });
        Assert.Equal(HttpStatusCode.Unauthorized, guest.StatusCode);
    }

    [Fact]
    public async Task Images_OtherAuthor403_UnknownImage404()
    {
        var owner = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(owner, "Bánh flan caramel");
        var other = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);

        using var form = new MultipartFormDataContent { { new ByteArrayContent(TestImages.Png()), "file", "a.png" } };
        form.First().Headers.ContentType = new("image/png");
        var forbidden = await other.PostAsync(new Uri($"/api/v1/recipes/{draft.Id}/images", UriKind.Relative), form);
        await forbidden.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);

        var unknown = await owner.PatchAsJsonAsync($"/api/v1/recipes/{draft.Id}/images/{Guid.NewGuid()}", new { altText = "x" });
        await unknown.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
    }

    /// <summary>IDX_RecipeImage_Primary không deferrable: đổi ảnh chính qua lại 10 lần liên tiếp không lần nào vỡ unique index.</summary>
    [Fact]
    public async Task SwitchPrimaryImage10Times_NeverViolatesUniqueIndex()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh mì chảo");
        var first = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "1.png");
        var second = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Jpeg(), "image/jpeg", "2.jpg");
        Assert.True(first.IsPrimary);
        Assert.False(second.IsPrimary);

        for (var i = 0; i < 10; i++)
        {
            var target = i % 2 == 0 ? second : first;
            var response = await author.PatchAsJsonAsync($"/api/v1/recipes/{draft.Id}/images/{target.ImageId}", new { isPrimary = true });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var primary = await db.RecipeImages.AsNoTracking().Where(i => i.RecipeId == draft.Id && i.IsPrimary).Select(i => i.Id).SingleAsync();
        Assert.Equal(first.ImageId, primary);
    }

    /// <summary>Xóa ảnh chính → xóa mềm, ảnh OrderIndex nhỏ nhất lên thay, tệp gốc được xếp hàng xóa qua Hangfire.</summary>
    [Fact]
    public async Task DeletePrimaryImage_PromotesNext_SoftDeletes_AndSchedulesFileDeletion()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh cuốn Thanh Trì");
        var first = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "1.png");
        var second = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Webp(), "image/webp", "2.webp");

        var deleted = await author.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}/images/{first.ImageId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.True(await db.RecipeImages.IgnoreQueryFilters().AnyAsync(i => i.Id == first.ImageId && i.IsDeleted));
        Assert.True(await db.RecipeImages.AnyAsync(i => i.Id == second.ImageId && i.IsPrimary));
        Assert.Contains(factory.BackgroundJobs.EnqueuedJobs, j =>
            j.JobType == typeof(DeleteStoredFilesJob)
            && j.Arguments[0] is IEnumerable<string> urls && urls.Contains(first.OriginalUrl));
    }

    /// <summary>Hai DbContext cùng sửa một công thức → lần lưu sau ném RecipeConcurrencyException (bộ dịch lỗi của Dev 2, Buổi 3).</summary>
    [Fact]
    public async Task TwoContextsEditingSameRecipe_SecondSaveThrowsRecipeConcurrencyException()
    {
        var draft = await RecipeApi.CreateDraftAsync(factory.CreateClientAs("Author"), "Hủ tiếu xào");

        using var scopeA = factory.Services.CreateScope();
        using var scopeB = factory.Services.CreateScope();
        var uowA = scopeA.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var uowB = scopeB.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var recipeA = (await uowA.Recipes.GetByIdWithDetailsAsync(draft.Id))!;
        var recipeB = (await uowB.Recipes.GetByIdWithDetailsAsync(draft.Id))!;

        recipeA.Update(recipeA.Title, recipeA.Description, recipeA.CategoryId, 11, recipeA.CookTimeMinutes, recipeA.Servings, recipeA.Difficulty, null);
        await uowA.SaveChangesAsync();
        recipeB.Update(recipeB.Title, recipeB.Description, recipeB.CategoryId, 22, recipeB.CookTimeMinutes, recipeB.Servings, recipeB.Difficulty, null);

        var ex = await Assert.ThrowsAsync<RecipeConcurrencyException>(() => uowB.SaveChangesAsync());
        Assert.Equal(draft.Id, ex.RecipeId);
    }
}
