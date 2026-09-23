using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>FR-CAT-001 / FR-CAT-002 – endpoint công khai, mỗi endpoint 1 happy path + 1 error case.</summary>
[Collection(IntegrationTestSuite.Name)]
public class CategoriesEndpointsTests(CulinaryBlogApiFactory factory)
{
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
}
