using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Lỗ hổng MT-34 (SRS v1.2.1) – cache của danh sách công thức được đánh key theo query string nhưng
/// NỘI DUNG lại phụ thuộc danh tính người gọi (<c>RecipeVisibility</c> trong <c>GetRecipesQuery</c>):
/// cùng một URL, Admin thấy cả Draft còn Guest chỉ thấy Published. Chừng nào cách ly cache chưa được
/// gỡ bỏ ở tầng thiết kế, hai người dùng khác quyền vẫn có thể chia nhau một entry cache.
///
/// Test được viết NGAY HÔM NAY và đánh dấu Skip có chủ đích: lỗ hổng phải nằm trong bộ test thay vì
/// nằm trong trí nhớ của ai đó. Retrofit D-4 + D-6 ở Buổi 6 gỡ lọc theo danh tính khỏi
/// <c>RecipeReadRepository</c> và xóa Output Cache — khi đó bỏ Skip và test này phải chuyển sang xanh.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeCacheIsolationTests(CulinaryBlogApiFactory factory)
{
    private const string SkipReason = "Lỗ hổng đã biết MT-34 — retrofit D-4/D-6 ở Buổi 6 (gỡ lọc theo danh tính + bỏ Output Cache).";

    [Fact(Skip = SkipReason)]
    public async Task GetRecipes_AfterAdminRequest_DoesNotLeakDraftsToGuest()
    {
        const string url = "/api/v1/recipes?page=1&pageSize=50";

        // Admin gọi trước: response của Admin bao gồm cả công thức Draft.
        var adminResponse = await factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId)
            .GetAsync(new Uri(url, UriKind.Relative));
        var adminPage = await adminResponse.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        Assert.Contains(adminPage.Items, r => r.Status == RecipeStatus.Draft);

        // Guest gọi LẠI CHÍNH URL ĐÓ: không được nhận nội dung của Admin.
        var guestResponse = await factory.CreateClient().GetAsync(new Uri(url, UriKind.Relative));
        var guestPage = await guestResponse.ReadAsAsync<PagedResult<RecipeSummaryDto>>();

        Assert.Equal(HttpStatusCode.OK, guestResponse.StatusCode);
        Assert.DoesNotContain(guestPage.Items, r => r.Status != RecipeStatus.Published);
        Assert.Equal(TestDataSeeder.PublishedRecipeCount, guestPage.TotalCount);
    }
}
