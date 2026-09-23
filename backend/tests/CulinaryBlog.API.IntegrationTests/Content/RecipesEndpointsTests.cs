using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>FR-RCP-001 / FR-RCP-002 – danh sách và chi tiết công thức (endpoint công khai của Buổi 2).</summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipesEndpointsTests(CulinaryBlogApiFactory factory)
{
    [Fact]
    public async Task GetRecipes_AsGuest_ReturnsOnlyPublished()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/recipes?page=1&pageSize=12", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        Assert.Equal(TestDataSeeder.PublishedRecipeCount, page.TotalCount);
        Assert.All(page.Items, r => Assert.Equal(RecipeStatus.Published, r.Status));
    }

    [Fact]
    public async Task GetRecipes_FilterByCategoryAndDifficulty_NarrowsResult()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            new Uri($"/api/v1/recipes?categoryId={TestDataSeeder.MonChinhCategoryId}&difficulty=Hard", UriKind.Relative));

        var page = await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var only = Assert.Single(page.Items);
        Assert.Equal(TestDataSeeder.PublishedRecipeSlug, only.Slug);
    }

    /// <summary>D-11 / MT-08: pageSize ngoài khoảng 1–50 là lỗi validation → 400 (không phải 422).</summary>
    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=500")]
    [InlineData("?sort=khong-ton-tai")]
    public async Task GetRecipes_WithInvalidQuery_Returns400(string query)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri($"/api/v1/recipes{query}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    [Fact]
    public async Task GetRecipeBySlug_Published_ReturnsDetailWithStepsAndIngredients()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri($"/api/v1/recipes/{TestDataSeeder.PublishedRecipeSlug}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal("Phở bò Hà Nội", detail.Title);
        Assert.NotEmpty(detail.Steps);
        Assert.NotEmpty(detail.Ingredients);
        Assert.Equal(1, detail.Steps[0].StepNumber);
    }

    [Fact]
    public async Task GetRecipeBySlug_WhenNotFound_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/recipes/khong-ton-tai-dau", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
    }

    /// <summary>
    /// FR-RCP-002 A2 ở trạng thái HIỆN TẠI: Draft của người khác → 403 RECIPE_FORBIDDEN.
    /// Buổi 6 (retrofit D-4) sẽ đổi thành 404 RECIPE_NOT_FOUND để không xác nhận sự tồn tại của slug;
    /// khi đó test này phải được cập nhật cùng commit đó.
    /// </summary>
    [Fact]
    public async Task GetRecipeBySlug_DraftAsGuest_Returns403ForNow()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri($"/api/v1/recipes/{TestDataSeeder.DraftRecipeSlug}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);
    }

    [Fact]
    public async Task GetRecipeBySlug_DraftAsOwner_Returns200()
    {
        var client = factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId);

        var response = await client.GetAsync(new Uri($"/api/v1/recipes/{TestDataSeeder.DraftRecipeSlug}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(RecipeStatus.Draft, (await response.ReadAsAsync<RecipeDetailDto>()).Status);
    }
}
