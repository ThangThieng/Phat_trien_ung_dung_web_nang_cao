using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// FR-RCP-001 / FR-RCP-002 – danh sách và chi tiết công thức (endpoint công khai của Buổi 2, sửa hợp đồng ở Buổi 4 — Dev 3:
/// chỉ Published, Draft → 404, sortBy/sortOrder thay cho sort).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipesEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

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

    /// <summary>
    /// D-11 / MT-08: lỗi validation → 400 (không phải 422). D-10 / MT-01: tham số <c>sort</c> cũ và giá trị ngoài whitelist
    /// của sortBy/sortOrder/difficulty → 400, không im lặng bỏ qua (FR-SRCH-003).
    /// </summary>
    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=500")]
    [InlineData("?pageSize=51")]
    [InlineData("?sort=khong-ton-tai")]
    [InlineData("?sort=-title")]
    [InlineData("?sortBy=password")]
    [InlineData("?sortBy=title&sortOrder=up")]
    [InlineData("?difficulty=Legendary")]
    [InlineData("?maxPrepTime=-1")]
    [InlineData("?minServings=0")]
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
    /// FR-RCP-002 A2 (retrofit D-4, MT-34): Draft qua endpoint công khai → 404 RECIPE_NOT_FOUND với CÙNG mã lỗi như slug không
    /// tồn tại — không xác nhận sự tồn tại của bản nháp (Buổi 2 trả 403). Áp dụng cho mọi người gọi, kể cả chủ sở hữu và Admin;
    /// chủ sở hữu xem bản nháp qua GET /recipes/mine/{id} (RecipeQueryEndpointsTests).
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("Author")]
    [InlineData("Admin")]
    public async Task GetRecipeBySlug_Draft_Returns404ForEveryCaller(string? role)
    {
        var client = role switch
        {
            null => factory.CreateClient(),
            "Admin" => factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId),
            { } other => factory.CreateClientAs(other, TestDataSeeder.AuthorUserId),
        };

        var response = await client.GetAsync(new Uri($"/api/v1/recipes/{TestDataSeeder.DraftRecipeSlug}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
    }

    /// <summary>FR-SRCH-003: sortBy + sortOrder sắp xếp đúng chiều.</summary>
    [Fact]
    public async Task GetRecipes_SortByTitleDesc_ReturnsDescendingOrder()
    {
        var response = await factory.CreateClient()
            .GetAsync(new Uri("/api/v1/recipes?sortBy=title&sortOrder=desc", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var titles = (await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>()).Items.Select(r => r.Title).ToList();
        Assert.Equal(TestDataSeeder.PublishedRecipeCount, titles.Count);
        Assert.Equal(titles.OrderByDescending(t => t, StringComparer.Ordinal).ToList(), titles);
    }

    /// <summary>FR-SRCH-002: difficulty nhận đủ 4 giá trị enum (Expert từng bị sót — MT-20.14).</summary>
    [Fact]
    public async Task GetRecipes_DifficultyExpert_Returns200()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes?difficulty=Expert", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>()).Items);
    }

    /// <summary>FR-SRCH-002: maxPrepTime và minServings kết hợp AND với các bộ lọc khác.</summary>
    [Fact]
    public async Task GetRecipes_MaxPrepTimeAndMinServings_NarrowResult()
    {
        // Dữ liệu test: Phở 40′, Bún chả 30′, Chè 15′ chuẩn bị; cả ba 4 khẩu phần.
        var response = await factory.CreateClient()
            .GetAsync(new Uri("/api/v1/recipes?maxPrepTime=30&minServings=4", UriKind.Relative));
        var page = await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>();

        Assert.Equal(2, page.TotalCount);
        Assert.All(page.Items, r => Assert.True(r.PrepTimeMinutes <= 30));

        var none = await factory.CreateClient()
            .GetAsync(new Uri("/api/v1/recipes?minServings=5", UriKind.Relative));
        Assert.Equal(0, (await none.ReadAsAsync<PagedResult<RecipeSummaryDto>>()).TotalCount);
    }

    /// <summary>FR-RCP-001 A3: categoryId không tồn tại → 200 với items rỗng (không 404).</summary>
    [Fact]
    public async Task GetRecipes_UnknownCategory_Returns200Empty()
    {
        var response = await factory.CreateClient()
            .GetAsync(new Uri($"/api/v1/recipes?categoryId={Guid.NewGuid()}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>()).Items);
    }

    /// <summary>FR-SRCH-004: phân trang offset — pageSize mặc định 12, cờ hasNextPage/hasPreviousPage đúng.</summary>
    [Fact]
    public async Task GetRecipes_Paging_ReportsNavigationFlags()
    {
        var first = await (await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes?pageSize=2", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        var second = await (await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes?pageSize=2&page=2", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        var defaults = await (await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSummaryDto>>();

        Assert.Equal(2, first.TotalPages);
        Assert.True(first.HasNextPage);
        Assert.False(first.HasPreviousPage);
        Assert.Single(second.Items);
        Assert.False(second.HasNextPage);
        Assert.True(second.HasPreviousPage);
        Assert.Empty(first.Items.Select(r => r.Id).Intersect(second.Items.Select(r => r.Id)));
        Assert.Equal(12, defaults.PageSize);
    }
}
