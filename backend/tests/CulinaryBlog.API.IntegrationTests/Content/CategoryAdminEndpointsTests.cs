using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// FR-CAT-003/004/005 – CRUD danh mục [Admin] qua HTTP thật trên Testcontainers (PostgreSQL + Redis + MinIO):
/// endpoint → MediatR → IUnitOfWork.Categories → PostgreSQL, kèm xóa cache Redis qua CacheInvalidationBehavior.
/// Mỗi test bắt đầu từ dữ liệu sạch (Respawn) và xóa khóa cache <c>categories:all</c> để không để lại danh sách cũ
/// cho các test khác trong cùng collection.
/// Dữ liệu nền (TestDataSeeder): "Món chính" (3 công thức chưa xóa: 2 Published + 1 Draft), "Tráng miệng" (1 công thức).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class CategoryAdminEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    private const string CategoriesUrl = "/api/v1/categories";

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        await ClearCategoryCacheAsync();
    }

    public Task DisposeAsync() => ClearCategoryCacheAsync();

    // ---------- FR-CAT-003 – POST /categories ----------
    [Fact]
    public async Task Create_AsAdmin_Returns201WithLocationAndGeneratedSlug()
    {
        var response = await AdminClient().PostAsJsonAsync(CategoriesUrl, new { name = "Món khai vị", description = "Mở đầu bữa ăn.", orderIndex = 3 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"{CategoriesUrl}/mon-khai-vi", response.Headers.Location?.ToString());

        var created = await response.ReadAsAsync<CategoryDto>();
        Assert.Equal("Món khai vị", created.Name);
        Assert.Equal("mon-khai-vi", created.Slug);
        Assert.Equal(3, created.OrderIndex);
        Assert.Equal(0, created.RecipeCount);

        var detail = await factory.CreateClient().GetAsync(new Uri($"{CategoriesUrl}/mon-khai-vi", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
    }

    [Fact]
    public async Task Create_AsAuthor_Returns403()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .PostAsJsonAsync(CategoriesUrl, new { name = "Món khai vị" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsGuest_Returns401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(CategoriesUrl, new { name = "Món khai vị" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("Món chính")]
    [InlineData("món chính")]
    public async Task Create_DuplicateName_Returns409(string name)
    {
        var response = await AdminClient().PostAsJsonAsync(CategoriesUrl, new { name });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.CategoryNameExists);
    }

    [Fact]
    public async Task Create_InvalidName_Returns400WithFieldErrors()
    {
        var response = await AdminClient().PostAsJsonAsync(CategoriesUrl, new { name = "a" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await response.ReadValidationErrorsAsync();
        Assert.True(errors.ContainsKey("name"), "Lỗi validation phải chỉ đúng trường 'name'.");
    }

    /// <summary>
    /// Hai Admin tạo cùng một tên gần như đồng thời: chỉ một request được ghi, request còn lại phải nhận 409 — từ bước kiểm tra
    /// chủ động hoặc từ bộ dịch lỗi 23505 (lớp phòng vệ thứ hai). Tuyệt đối không có 500.
    /// </summary>
    [Fact]
    public async Task Create_SameNameInParallel_ReturnsOneCreatedAndOneConflict()
    {
        var first = AdminClient();
        var second = AdminClient();
        var payload = new { name = "Món chay" };

        var responses = await Task.WhenAll(
            first.PostAsJsonAsync(CategoriesUrl, payload),
            second.PostAsJsonAsync(CategoriesUrl, payload));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        var conflict = Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        await conflict.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.CategoryNameExists);
        Assert.DoesNotContain(responses, r => r.StatusCode >= HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Lớp phòng vệ thứ hai kiểm tra trực tiếp, không phụ thuộc thời điểm của hai request: bỏ qua bước kiểm tra chủ động và ghi
    /// hai danh mục cùng tên qua IUnitOfWork → PostgreSQL 23505 phải được dịch thành lỗi nghiệp vụ (không phải DbUpdateException).
    /// </summary>
    [Fact]
    public async Task UnitOfWork_DuplicateNameInsert_IsTranslatedToCategoryNameAlreadyExists()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.Categories.AddAsync(Category.Create("Món nướng", "mon-nuong"));
            await unitOfWork.SaveChangesAsync();
        }

        using var secondScope = factory.Services.CreateScope();
        var secondUnitOfWork = secondScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await secondUnitOfWork.Categories.AddAsync(Category.Create("Món nướng", "mon-nuong-khac"));

        var error = await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(() => secondUnitOfWork.SaveChangesAsync());
        Assert.Equal(ErrorCodes.CategoryNameExists, error.Code);
    }

    [Fact]
    public async Task Create_InvalidatesCachedCategoryList()
    {
        var client = AdminClient();
        var before = await ReadListAsync();
        Assert.DoesNotContain(before, c => c.Slug == "mon-khai-vi");

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(CategoriesUrl, new { name = "Món khai vị" })).StatusCode);

        Assert.Contains(await ReadListAsync(), c => c.Slug == "mon-khai-vi");
    }

    // ---------- Danh mục đã xóa mềm vẫn giữ tên và slug (IDX_Category_Name / IDX_Category_Slug là unique thường) ----------
    [Fact]
    public async Task Create_NameOfSoftDeletedCategory_Returns409NotServerError()
    {
        var client = AdminClient();
        var id = await CreateCategoryAsync(client, "Món chay");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(new Uri($"{CategoriesUrl}/{id}", UriKind.Relative))).StatusCode);

        var response = await client.PostAsJsonAsync(CategoriesUrl, new { name = "Món chay" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.CategoryNameExists);
    }

    [Fact]
    public async Task Create_DifferentNameWithSlugOfSoftDeletedCategory_GetsSuffixedSlug()
    {
        var client = AdminClient();
        var id = await CreateCategoryAsync(client, "Món chay");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(new Uri($"{CategoriesUrl}/{id}", UriKind.Relative))).StatusCode);

        // "Món-chay" khác tên "Món chay" nhưng sinh cùng slug "mon-chay" với danh mục đã xóa mềm.
        var response = await client.PostAsJsonAsync(CategoriesUrl, new { name = "Món-chay" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("mon-chay-2", (await response.ReadAsAsync<CategoryDto>()).Slug);
    }

    // ---------- FR-CAT-004 – PUT /categories/{id} ----------
    [Fact]
    public async Task Update_Rename_KeepsSlug()
    {
        var response = await AdminClient().PutAsJsonAsync(
            $"{CategoriesUrl}/{TestDataSeeder.TrangMiengCategoryId}",
            new { name = "Món ngọt", description = "Mô tả mới", orderIndex = 5 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.ReadAsAsync<CategoryDto>();
        Assert.Equal("Món ngọt", updated.Name);
        Assert.Equal("trang-mieng", updated.Slug);
        Assert.Equal(5, updated.OrderIndex);
        Assert.Equal(1, updated.RecipeCount);

        // Link cũ (slug) vẫn còn sống.
        var byOldSlug = await factory.CreateClient().GetAsync(new Uri($"{CategoriesUrl}/trang-mieng", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, byOldSlug.StatusCode);
    }

    [Fact]
    public async Task Update_KeepingOwnName_Returns200()
    {
        var response = await AdminClient().PutAsJsonAsync(
            $"{CategoriesUrl}/{TestDataSeeder.MonChinhCategoryId}",
            new { name = "Món chính", description = "Chỉ đổi mô tả." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ToNameOfAnotherCategory_Returns409()
    {
        var response = await AdminClient().PutAsJsonAsync(
            $"{CategoriesUrl}/{TestDataSeeder.TrangMiengCategoryId}",
            new { name = "Món chính" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.CategoryNameExists);
    }

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        var response = await AdminClient().PutAsJsonAsync($"{CategoriesUrl}/{Guid.NewGuid()}", new { name = "Món ngọt" });

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.CategoryNotFound);
    }

    [Fact]
    public async Task Update_AsAuthor_Returns403()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .PutAsJsonAsync($"{CategoriesUrl}/{TestDataSeeder.TrangMiengCategoryId}", new { name = "Món ngọt" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- FR-CAT-005 – DELETE /categories/{id} ----------
    [Fact]
    public async Task Delete_CategoryWithRecipes_Returns409WithRecipeCountExtension()
    {
        var response = await AdminClient().DeleteAsync(new Uri($"{CategoriesUrl}/{TestDataSeeder.MonChinhCategoryId}", UriKind.Relative));

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.CategoryDeleteHasRecipes);

        // "Món chính" có 3 công thức chưa xóa (2 Published + 1 Draft) — Draft cũng chặn xóa.
        var recipeCount = Assert.IsType<JsonElement>(problem.Extensions["recipeCount"]);
        Assert.Equal(3, recipeCount.GetInt32());
        Assert.Contains("3", problem.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Delete_EmptyCategory_Returns204ThenGetReturns404()
    {
        var client = AdminClient();
        var id = await CreateCategoryAsync(client, "Món chay");

        var delete = await client.DeleteAsync(new Uri($"{CategoriesUrl}/{id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await factory.CreateClient().GetAsync(new Uri($"{CategoriesUrl}/mon-chay", UriKind.Relative));
        await get.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.CategoryNotFound);
        Assert.DoesNotContain(await ReadListAsync(), c => c.Slug == "mon-chay");
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var response = await AdminClient().DeleteAsync(new Uri($"{CategoriesUrl}/{Guid.NewGuid()}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.CategoryNotFound);
    }

    [Fact]
    public async Task Delete_AsAuthor_Returns403()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .DeleteAsync(new Uri($"{CategoriesUrl}/{TestDataSeeder.TrangMiengCategoryId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>Tài khoản Admin được seed cả hai role (SRS §2.3, MT-33.5) nên token test mang "Author,Admin".</summary>
    private HttpClient AdminClient() => factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId);

    private static async Task<Guid> CreateCategoryAsync(HttpClient admin, string name)
    {
        var response = await admin.PostAsJsonAsync(CategoriesUrl, new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.ReadAsAsync<CategoryDto>()).Id;
    }

    private async Task<List<CategoryDto>> ReadListAsync()
    {
        var response = await factory.CreateClient().GetAsync(new Uri(CategoriesUrl, UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsAsync<List<CategoryDto>>();
    }

    private async Task ClearCategoryCacheAsync()
    {
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ICacheService>().RemoveAsync(CategoryCacheKeys.All);
    }
}
