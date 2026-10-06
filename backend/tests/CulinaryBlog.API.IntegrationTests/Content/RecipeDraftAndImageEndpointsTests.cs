using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Amazon.S3;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Recipes;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Buổi 3 — Dev 2: FR-RCP-003 tạo công thức nháp và FR-RCP-008 quản lý ảnh trên PostgreSQL + MinIO thật — gồm đổi ảnh
/// chính trong một transaction hai bước (IDX_RecipeImage_Primary không deferrable), xóa mềm ảnh + xếp hàng xóa tệp qua
/// Hangfire, magic bytes, và bộ dịch lỗi RowVersion của aggregate Recipe.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeDraftAndImageEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- FR-RCP-003 ----------
    [Fact]
    public async Task CreateDraft_Returns201AsDraftWithRealCategoryAndAuthorNames()
    {
        var author = factory.CreateClientAs("Author");

        var response = await author.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Gà nướng mật ong",
            description = "Gà nướng mật ong vàng óng, thơm mùi tỏi và sả.",
            categoryId = TestDataSeeder.MonChinhCategoryId,
            prepTime = 20,
            cookTime = 45,
            servings = 4,
            difficulty = "Medium",
            nutrition = new { calories = 420, protein = 35 },
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var draft = await response.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal(RecipeStatus.Draft, draft.Status);
        Assert.Null(draft.PublishedAt);
        Assert.Null(draft.Instructions); // D-8: không gửi instructions vẫn tạo được (cột đã NULL)
        Assert.Equal("ga-nuong-mat-ong", draft.Slug);
        Assert.Equal("Món chính", draft.Category.Name);
        Assert.Equal("IT Author", draft.Author.DisplayName);
        Assert.Equal(420, draft.Nutrition?.Calories);
    }

    [Fact]
    public async Task CreateDraft_InvalidBody400WithFieldErrors_UnknownCategory400OnField_Guest401()
    {
        var author = factory.CreateClientAs("Author");

        var invalid = await author.PostAsJsonAsync("/api/v1/recipes", new { title = "Ab", description = "ngắn", categoryId = TestDataSeeder.MonChinhCategoryId, prepTime = 0, cookTime = 10, servings = 1, difficulty = "Easy" });
        await invalid.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        var errors = await invalid.ReadValidationErrorsAsync();
        Assert.Contains("title", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("description", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("prepTime", errors.Keys, StringComparer.Ordinal);

        // FR-RCP-003 A2 (MT-61): categoryId là một trường của body → 400 gắn vào "categoryId", không phải 404.
        var unknownCategory = await author.PostAsJsonAsync("/api/v1/recipes", new { title = "Lẩu thái chua cay", description = "Lẩu thái chua cay hải sản cho cả nhà.", categoryId = Guid.NewGuid(), prepTime = 20, cookTime = 30, servings = 4, difficulty = "Medium" });
        await unknownCategory.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("categoryId", (await unknownCategory.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);

        var guest = await factory.CreateClient().PostAsJsonAsync("/api/v1/recipes", new { title = "Lẩu thái chua cay" });
        Assert.Equal(HttpStatusCode.Unauthorized, guest.StatusCode);
    }

    /// <summary>Tiêu đề trùng từ khóa dành riêng (NFR-SEO-004) → thêm hậu tố như slug trùng, không từ chối request.</summary>
    [Fact]
    public async Task CreateDraft_WithReservedSlugTitle_GetsSuffix()
    {
        var draft = await RecipeApi.CreateDraftAsync(factory.CreateClientAs("Author"), "Search");

        Assert.Equal("search-2", draft.Slug);
    }

    /// <summary>Slug trùng công thức đã có → hậu tố -2 (không nổ 23505 → 500).</summary>
    [Fact]
    public async Task CreateDraft_WithExistingSlug_GetsSuffix()
    {
        var draft = await RecipeApi.CreateDraftAsync(factory.CreateClientAs("Author"), "Phở bò Hà Nội");

        Assert.Equal($"{TestDataSeeder.PublishedRecipeSlug}-2", draft.Slug);
    }

    // ---------- FR-RCP-008 ----------
    [Fact]
    public async Task Upload_ReturnsSevenFieldContract_AndStoresUnderRecipeFolder()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh cuốn Thanh Trì");

        var image = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "banh-cuon.png");

        Assert.NotEqual(Guid.Empty, image.ImageId);
        Assert.True(image.IsPrimary);
        Assert.Null(image.MediumUrl);
        Assert.Null(image.ThumbnailUrl);
        Assert.Contains($"/recipes/{draft.Id}/", image.OriginalUrl, StringComparison.Ordinal);
        Assert.EndsWith(".png", image.OriginalUrl, StringComparison.Ordinal);
        Assert.DoesNotContain("banh-cuon", image.OriginalUrl, StringComparison.Ordinal);

        using var scope = factory.Services.CreateScope();
        Assert.True(await MinioObject.ExistsAsync(scope.ServiceProvider.GetRequiredService<IAmazonS3>(), image.OriginalUrl));
    }

    /// <summary>NFR-SEC-004: Content-Type khai báo là ảnh nhưng magic bytes là văn bản → 400; file rỗng không phải "quá lớn".</summary>
    [Fact]
    public async Task Upload_FakedContentType400_EmptyFile400MimeInvalid()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh flan caramel");

        var faked = await PostImageAsync(author, draft.Id, TestImages.NotAnImage(), "image/png");
        await faked.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.FileMimeInvalid);

        var empty = await PostImageAsync(author, draft.Id, [], "image/png");
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.NotEqual(ErrorCodes.FileSizeExceeded, (await empty.ReadAsAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>()).Type);
    }

    [Fact]
    public async Task Images_OtherAuthor403_UnknownImage404_UnknownRecipe404()
    {
        var owner = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(owner, "Bánh bèo chén");
        var other = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);

        var forbidden = await PostImageAsync(other, draft.Id, TestImages.Png(), "image/png");
        await forbidden.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);

        var unknownImage = await owner.PatchAsJsonAsync($"/api/v1/recipes/{draft.Id}/images/{Guid.NewGuid()}", new { altText = "x" });
        await unknownImage.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);

        var unknownRecipe = await PostImageAsync(owner, Guid.NewGuid(), TestImages.Png(), "image/png");
        await unknownRecipe.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
    }

    /// <summary>Admin thao tác được trên ảnh của mọi công thức (RecipeAuthorizationHandler: chủ sở hữu HOẶC Admin).</summary>
    [Fact]
    public async Task Images_AdminCanManageAnyRecipe()
    {
        var draft = await RecipeApi.CreateDraftAsync(factory.CreateClientAs("Author"), "Chè bưởi An Giang");
        var admin = factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);

        var response = await PostImageAsync(admin, draft.Id, TestImages.Webp(), "image/webp");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>FR-RCP-008 A4: MinIO không khả dụng → 503 FILE_STORAGE_UNAVAILABLE.</summary>
    [Fact]
    public async Task Upload_WhenStorageUnavailable_Returns503()
    {
        using var brokenStorage = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<IFileStorageService, UnavailableStorage>()));
        var author = brokenStorage.CreateClient();
        author.DefaultRequestHeaders.Authorization = factory.CreateClientAs("Author").DefaultRequestHeaders.Authorization;
        var draft = await RecipeApi.CreateDraftAsync(author, "Cơm tấm sườn bì");

        var response = await PostImageAsync(author, draft.Id, TestImages.Png(), "image/png");

        await response.ShouldBeProblemAsync(HttpStatusCode.ServiceUnavailable, ErrorCodes.FileStorageUnavailable);
    }

    /// <summary>IDX_RecipeImage_Primary không deferrable: đổi ảnh chính qua lại 10 lần liên tiếp không lần nào vỡ unique index.</summary>
    [Fact]
    public async Task SwitchPrimaryImage10Times_NeverViolatesUniqueIndex()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh mì chảo");
        var first = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "1.png");
        var second = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Jpeg(), "image/jpeg", "2.jpg");

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

    /// <summary>Tải ảnh mới với isPrimary = true khi đã có ảnh chính → ảnh mới thành ảnh chính duy nhất.</summary>
    [Fact]
    public async Task UploadWithIsPrimary_ReplacesCurrentPrimary()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bò lá lốt");
        await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "1.png");

        using var form = new MultipartFormDataContent
        {
            { new ByteArrayContent(TestImages.Jpeg()) { Headers = { ContentType = new MediaTypeHeaderValue("image/jpeg") } }, "file", "2.jpg" },
            { new StringContent("true"), "isPrimary" },
            { new StringContent("Bò lá lốt nướng than"), "altText" },
        };
        var response = await author.PostAsync(new Uri($"/api/v1/recipes/{draft.Id}/images", UriKind.Relative), form);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.ReadAsAsync<RecipeImageResultDto>();
        Assert.True(created.IsPrimary);
        Assert.Equal("Bò lá lốt nướng than", created.AltText);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.Equal(1, await db.RecipeImages.CountAsync(i => i.RecipeId == draft.Id && i.IsPrimary));
    }

    /// <summary>Xóa ảnh chính → xóa mềm, ảnh OrderIndex nhỏ nhất lên thay, tệp gốc được xếp hàng xóa qua Hangfire (SRS §8.4).</summary>
    [Fact]
    public async Task DeletePrimaryImage_PromotesNext_SoftDeletes_AndSchedulesFileDeletion()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh cuốn nóng");
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

    /// <summary>
    /// Hai DbContext cùng sửa một ảnh của công thức → lần lưu sau ném RecipeConcurrencyException (409), không phải 500:
    /// bộ dịch lỗi nhận ra entry con thuộc aggregate Recipe và gắn đúng RecipeId.
    /// </summary>
    [Fact]
    public async Task TwoContextsEditingSameRecipeImage_SecondSaveThrowsRecipeConcurrencyException()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Hủ tiếu xào");
        var image = await RecipeApi.UploadImageAsync(author, draft.Id, TestImages.Png(), "image/png", "hu-tieu.png");

        using var scopeA = factory.Services.CreateScope();
        using var scopeB = factory.Services.CreateScope();
        var uowA = scopeA.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var uowB = scopeB.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var recipeA = (await uowA.Recipes.GetByIdWithImagesAsync(draft.Id))!;
        var recipeB = (await uowB.Recipes.GetByIdWithImagesAsync(draft.Id))!;

        recipeA.UpdateImageMetadata(image.ImageId, "Hủ tiếu xào giòn", orderIndex: null);
        await uowA.SaveChangesAsync();
        recipeB.UpdateImageMetadata(image.ImageId, "Hủ tiếu xào mềm", orderIndex: null);

        var ex = await Assert.ThrowsAsync<RecipeConcurrencyException>(() => uowB.SaveChangesAsync());
        Assert.Equal(draft.Id, ex.RecipeId);
    }

    /// <summary>Hai DbContext cùng sửa một công thức → lần lưu sau ném RecipeConcurrencyException (mức Recipe — Recipe.Update của FR-RCP-004).</summary>
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

    /// <summary>DELETE ảnh: ảnh không tồn tại → 404, người khác → 403; ảnh của người khác không bị đụng tới.</summary>
    [Fact]
    public async Task DeleteImage_Unknown404_OtherAuthor403()
    {
        var owner = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(owner, "Bánh khọt Vũng Tàu");
        var image = await RecipeApi.UploadImageAsync(owner, draft.Id, TestImages.Png(), "image/png", "banh-khot.png");
        var other = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);

        var unknown = await owner.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}/images/{Guid.NewGuid()}", UriKind.Relative));
        await unknown.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);

        var byOther = await other.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}/images/{image.ImageId}", UriKind.Relative));
        await byOther.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        Assert.True(await db.RecipeImages.AnyAsync(i => i.Id == image.ImageId));
    }

    private static async Task<HttpResponseMessage> PostImageAsync(HttpClient client, Guid recipeId, byte[] content, string contentType)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", "anh.png");
        return await client.PostAsync(new Uri($"/api/v1/recipes/{recipeId}/images", UriKind.Relative), form);
    }

    /// <summary>MinIO "sập": mọi thao tác trả đúng lỗi mà MinioFileStorageService trả khi không kết nối được.</summary>
    private sealed class UnavailableStorage : IFileStorageService
    {
        public Task<StoredFile> UploadAsync(Stream content, string folder, string extension, string contentType, CancellationToken cancellationToken = default) =>
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");

        public Task DeleteAsync(string fileUrlOrKey, CancellationToken cancellationToken = default) =>
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");

        public Task<Stream> OpenReadAsync(string fileUrlOrKey, CancellationToken cancellationToken = default) =>
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");

        public bool IsStoredFileUrl(string url) => false;
    }
}
