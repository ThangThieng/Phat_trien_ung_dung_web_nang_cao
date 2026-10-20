using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Lỗ hổng MT-34 — ĐÃ VÁ ở Buổi 4 (Dev 3, retrofit D-4/D-5/D-6). Ở Buổi 2, danh sách công thức vừa lọc theo danh tính (Admin
/// thấy cả Draft) vừa bị Output Cache lưu dưới khóa chỉ gồm query string: một lượt truy cập của Admin nạp Draft vào cache, mọi
/// Guest gọi cùng URL đọc được. Test này được Dev 4 viết từ Buổi 3 ở trạng thái Skip; nay bỏ Skip và phải xanh.
/// Khẳng định hiện tại: endpoint công khai chỉ trả Published cho MỌI người gọi (kể cả Admin), nên thứ tự Admin → Guest trên cùng
/// một URL (cache Redis dùng chung) không thể làm lộ bản nháp.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeCacheIsolationTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRecipes_AfterAdminRequest_DoesNotLeakDraftsToGuest()
    {
        const string url = "/api/v1/recipes?page=1&pageSize=50";

        // Admin gọi trước — và cũng chỉ thấy Published (FR-RCP-001: "cho mọi người gọi, không ngoại lệ — kể cả Admin").
        var adminResponse = await factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId)
            .GetAsync(new Uri(url, UriKind.Relative));
        var adminPage = await adminResponse.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.All(adminPage.Items, r => Assert.Equal(RecipeStatus.Published, r.Status));

        // Guest gọi LẠI CHÍNH URL ĐÓ (trúng cache Redis vừa được lượt của Admin ghi): không có bản nháp nào.
        var guestResponse = await factory.CreateClient().GetAsync(new Uri(url, UriKind.Relative));
        var guestPage = await guestResponse.ReadAsAsync<PagedResult<RecipeSummaryDto>>();

        Assert.Equal(HttpStatusCode.OK, guestResponse.StatusCode);
        Assert.DoesNotContain(guestPage.Items, r => r.Status != RecipeStatus.Published);
        Assert.DoesNotContain(guestPage.Items, r => r.Slug == TestDataSeeder.DraftRecipeSlug);
        Assert.Equal(TestDataSeeder.PublishedRecipeCount, guestPage.TotalCount);
    }

    /// <summary>D-5: chi tiết danh mục cũng không còn lọc theo danh tính — chủ bản nháp không thấy bản nháp của mình ở đây.</summary>
    [Fact]
    public async Task GetCategoryBySlug_AsDraftOwner_ReturnsOnlyPublished()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .GetAsync(new Uri("/api/v1/categories/mon-chinh?page=1&pageSize=50", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.ReadAsAsync<CategoryDetailDto>();
        Assert.Equal(2, detail.Recipes.TotalCount);
        Assert.All(detail.Recipes.Items, r => Assert.Equal(RecipeStatus.Published, r.Status));
    }

    /// <summary>D-4: bản nháp qua endpoint chi tiết công khai → 404 cho cả chủ sở hữu (không chỉ Guest).</summary>
    [Fact]
    public async Task GetRecipeBySlug_DraftAsOwner_Returns404()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .GetAsync(new Uri($"/api/v1/recipes/{TestDataSeeder.DraftRecipeSlug}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
