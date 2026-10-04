using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>FR-CAT-001 / FR-CAT-002 – endpoint công khai, mỗi endpoint 1 happy path + 1 error case.</summary>
[Collection(IntegrationTestSuite.Name)]
public class CategoriesEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCategories_AsGuest_ReturnsListWithPublishedRecipeCount()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/categories", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.ReadAsAsync<List<CategoryDto>>();
        var monChinh = Assert.Single(categories, c => c.Slug == "mon-chinh");

        // RecipeCount chỉ đếm Published: "Món chính" có 2 Published + 1 Draft.
        Assert.Equal(2, monChinh.RecipeCount);
    }

    [Fact]
    public async Task GetCategoryBySlug_ReturnsCategoryWithPagedRecipes()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/categories/mon-chinh?page=1&pageSize=12", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.ReadAsAsync<CategoryDetailDto>();
        Assert.Equal("Món chính", detail.Category.Name);
        Assert.Equal(2, detail.Recipes.TotalCount);
        Assert.All(detail.Recipes.Items, r => Assert.Equal(TestDataSeeder.MonChinhCategoryId, r.Category.Id));
    }

    [Fact]
    public async Task GetCategoryBySlug_WhenNotFound_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/categories/khong-ton-tai", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.CategoryNotFound);
    }

    /// <summary>FR-CAT-002 (Buổi 4 — D-10 phía API): sortBy/sortOrder theo whitelist FR-SRCH-003.</summary>
    [Fact]
    public async Task GetCategoryBySlug_SortByTitleAsc_ReturnsAscendingOrder()
    {
        var response = await factory.CreateClient()
            .GetAsync(new Uri("/api/v1/categories/mon-chinh?sortBy=title&sortOrder=asc", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var titles = (await response.ReadAsAsync<CategoryDetailDto>()).Recipes.Items.Select(r => r.Title).ToList();
        Assert.Equal(titles.Order(StringComparer.Ordinal).ToList(), titles);
    }

    /// <summary>FR-CAT-002 A2: tham số phân trang/sắp xếp không hợp lệ → 400.</summary>
    [Theory]
    [InlineData("?sortBy=name")]
    [InlineData("?sortOrder=sideways")]
    [InlineData("?pageSize=51")]
    public async Task GetCategoryBySlug_InvalidQuery_Returns400(string query)
    {
        var response = await factory.CreateClient().GetAsync(new Uri($"/api/v1/categories/mon-chinh{query}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }
}
