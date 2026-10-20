using System.Net;
using Amazon.S3;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Buổi 4 — Dev 4: FR-RCP-005/006/007 trên PostgreSQL + MinIO thật — máy trạng thái khép kín (D-15), xóa mềm và
/// partial unique slug (D-3). Mỗi endpoint có luồng thành công + các nhánh lỗi 400/401/403/404/409 (NFR-MAINT-002).
/// Kiểm tra "đã ẩn" dùng client có token: request có Authorization không đi qua Output Cache cũ (nợ D-6 của Dev 3).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeLifecycleEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Publish_DraftWithStepsAndIngredients_Returns200AndSetsPublishedAt()
    {
        var author = factory.CreateClientAs("Author");
        var draftId = await IdOfSeededAsync(TestDataSeeder.DraftRecipeSlug);

        var response = await RecipeApi.PatchAsync(author, draftId, "publish");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recipe = await response.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.NotNull(recipe.PublishedAt);
        Assert.False(string.IsNullOrEmpty(recipe.RowVersion));
    }

    /// <summary>FR-RCP-005 A1: công thức vừa tạo chưa có bước lẫn nguyên liệu → 400, extension "missing" nêu rõ thiếu gì.</summary>
    [Fact]
    public async Task Publish_RecipeWithoutContent_Returns400PublishIncomplete()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Canh chua cá lóc");

        var response = await RecipeApi.PatchAsync(author, draft.Id, "publish");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.RecipePublishIncomplete);
        Assert.Contains("ingredients", problem.Extensions["missing"]!.ToString(), StringComparison.Ordinal);
        Assert.Contains("steps", problem.Extensions["missing"]!.ToString(), StringComparison.Ordinal);
    }

    /// <summary>MT-35: Published → Archived → (publish bị từ chối 409) → unarchive về Draft; PublishedAt giữ nguyên suốt chuỗi.</summary>
    [Fact]
    public async Task ArchiveThenPublish_Returns409_AndUnarchiveReturnsToDraft()
    {
        var author = factory.CreateClientAs("Author");
        var id = await IdOfSeededAsync(TestDataSeeder.PublishedRecipeSlug);

        var archived = await RecipeApi.PatchAsync(author, id, "archive");
        Assert.Equal(HttpStatusCode.OK, archived.StatusCode);
        var archivedRecipe = await archived.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal(RecipeStatus.Archived, archivedRecipe.Status);

        var publishArchived = await RecipeApi.PatchAsync(author, id, "publish");
        var problem = await publishArchived.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.RecipeInvalidStateTransition);
        Assert.Equal("Archived", problem.Extensions["currentStatus"]!.ToString());

        var unarchived = await RecipeApi.PatchAsync(author, id, "unarchive");
        Assert.Equal(HttpStatusCode.OK, unarchived.StatusCode);
        var draft = await unarchived.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal(RecipeStatus.Draft, draft.Status);
        Assert.Equal(archivedRecipe.PublishedAt, draft.PublishedAt);
    }

    [Theory]
    [InlineData("unpublish")]
    [InlineData("unarchive")]
    public async Task InvalidTransitionFromDraft_Returns409(string action)
    {
        var author = factory.CreateClientAs("Author");
        var draftId = await IdOfSeededAsync(TestDataSeeder.DraftRecipeSlug);

        var response = await RecipeApi.PatchAsync(author, draftId, action);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.RecipeInvalidStateTransition);
    }

    [Fact]
    public async Task Lifecycle_ByAnotherAuthor_Returns403_ButAdminIsAllowed()
    {
        var otherAuthor = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);
        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);
        var id = await IdOfSeededAsync(TestDataSeeder.PublishedRecipeSlug);

        var forbidden = await RecipeApi.PatchAsync(otherAuthor, id, "unpublish");
        await forbidden.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);

        var byAdmin = await RecipeApi.PatchAsync(admin, id, "unpublish");
        Assert.Equal(HttpStatusCode.OK, byAdmin.StatusCode);
    }

    [Fact]
    public async Task Lifecycle_UnknownRecipe_Returns404_AndGuestReturns401()
    {
        var author = factory.CreateClientAs("Author");

        var notFound = await RecipeApi.PatchAsync(author, Guid.NewGuid(), "archive");
        await notFound.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);

        var guest = await RecipeApi.PatchAsync(factory.CreateClient(), await IdOfSeededAsync(TestDataSeeder.PublishedRecipeSlug), "archive");
        Assert.Equal(HttpStatusCode.Unauthorized, guest.StatusCode);
    }

    /// <summary>
    /// FR-RCP-007 + MT-05: xóa mềm → 204; công thức biến mất khỏi mọi truy vấn (lần xóa thứ hai 404), nhưng bản ghi,
    /// bản ghi con và TỆP ẢNH TRÊN MINIO vẫn còn — khôi phục được cho tới khi PermanentPurgeJob chạy.
    /// </summary>
    [Fact]
    public async Task Delete_SoftDeletesRecipe_KeepsChildrenAndStoredFiles()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Gà kho gừng");
        var image = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "ga-kho.png");

        var deleted = await author.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var deleteAgain = await author.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}", UriKind.Relative));
        await deleteAgain.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);

        var bySlug = await author.GetAsync(new Uri($"/api/v1/recipes/{draft.Slug}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, bySlug.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var row = await db.Recipes.IgnoreQueryFilters().AsNoTracking().SingleAsync(r => r.Id == draft.Id);
            Assert.True(row.IsDeleted);
            Assert.Equal(RecipeStatus.Draft, row.Status);
            Assert.True(await db.RecipeImages.AnyAsync(i => i.Id == image.ImageId), "Ảnh phải còn nguyên (chỉ vô hình cùng công thức).");
            Assert.False(await db.RecipeImages.IgnoreQueryFilters().AnyAsync(i => i.Id == image.ImageId && i.IsDeleted));
        }

        // MT-05 hệ quả 3: KHÔNG xóa tệp khi xóa mềm — object vẫn tải được từ MinIO.
        Assert.DoesNotContain(factory.BackgroundJobs.EnqueuedJobs, j => j.JobType == typeof(DeleteStoredFilesJob));
        await AssertObjectExistsAsync(image.OriginalUrl);
    }

    [Fact]
    public async Task Delete_ByAnotherAuthor_Returns403()
    {
        var otherAuthor = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);
        var id = await IdOfSeededAsync(TestDataSeeder.PublishedRecipeSlug);

        var response = await otherAuthor.DeleteAsync(new Uri($"/api/v1/recipes/{id}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);
    }

    /// <summary>D-3: IDX_Recipe_Slug là partial unique → xóa "Phở bò tái lăn" rồi tạo lại cùng tên nhận lại đúng slug cũ.</summary>
    [Fact]
    public async Task DeleteThenRecreateSameTitle_ReusesSlug()
    {
        var author = factory.CreateClientAs("Author");
        var first = await RecipeApi.CreateDraftAsync(author, "Phở bò tái lăn");
        Assert.Equal("pho-bo-tai-lan", first.Slug);

        var deleted = await author.DeleteAsync(new Uri($"/api/v1/recipes/{first.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var second = await RecipeApi.CreateDraftAsync(author, "Phở bò tái lăn");

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal("pho-bo-tai-lan", second.Slug);
    }

    private async Task<Guid> IdOfSeededAsync(string slug)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        return await db.Recipes.AsNoTracking().Where(r => r.Slug == slug).Select(r => r.Id).SingleAsync();
    }

    private async Task AssertObjectExistsAsync(string url)
    {
        using var scope = factory.Services.CreateScope();
        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        var (bucket, key) = MinioObject.Parse(url);
        var metadata = await s3.GetObjectMetadataAsync(bucket, key);
        Assert.Equal(HttpStatusCode.OK, metadata.HttpStatusCode);
    }
}
