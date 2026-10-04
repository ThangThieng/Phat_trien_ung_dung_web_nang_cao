using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Buổi 4 — Dev 3: bốn endpoint đọc mới của SRS v1.2.2 §8.3 — tìm kiếm toàn văn (FR-SRCH-001), công thức của tôi (FR-RCP-011),
/// chi tiết riêng tư theo id (CR-2026-04 d / MT-62) và nguồn sitemap (MT-48). Mỗi endpoint ≥ 1 happy path + ≥ 1 error case
/// (NFR-MAINT-002). Dữ liệu: <see cref="TestDataSeeder"/> — 3 công thức Published + 1 Draft, cùng một tác giả.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeQueryEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    private const int SeededRecipesOfAuthor = 4;

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- FR-SRCH-001 – GET /recipes/search ----------

    /// <summary>Tiêu chí nghiệm thu Buổi 4: gõ "pho" (không dấu) tìm được "Phở bò".</summary>
    [Theory]
    [InlineData("pho")]
    [InlineData("pho bo")]
    [InlineData("PHỞ Bò")]
    public async Task Search_Unaccented_FindsPhoBo(string q)
    {
        var response = await factory.CreateClient()
            .GetAsync(new Uri($"/api/v1/recipes/search?q={Uri.EscapeDataString(q)}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.ReadAsAsync<PagedResult<RecipeSearchResultDto>>();
        var hit = Assert.Single(page.Items, r => r.Slug == TestDataSeeder.PublishedRecipeSlug);
        Assert.True(hit.RelevanceScore > 0);
    }

    [Fact]
    public async Task Search_OrdersByRelevance_AndReturnsOnlyPublished()
    {
        var page = await (await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/search?q=ha+noi", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSearchResultDto>>();

        Assert.Equal(2, page.TotalCount);
        Assert.All(page.Items, r => Assert.Equal(RecipeStatus.Published, r.Status));
        Assert.Equal(page.Items.OrderByDescending(r => r.RelevanceScore).Select(r => r.Id), page.Items.Select(r => r.Id));

        // Bản nháp "Bánh mì thịt nướng (nháp)" có trong DB nhưng không bao giờ xuất hiện ở tìm kiếm công khai (MT-34).
        var draft = await (await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/search?q=banh+mi", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSearchResultDto>>();
        Assert.Empty(draft.Items);
    }

    [Fact]
    public async Task Search_WithCategoryFilter_CombinesWithAnd()
    {
        var response = await factory.CreateClient().GetAsync(
            new Uri($"/api/v1/recipes/search?q=ha+noi&categoryId={TestDataSeeder.TrangMiengCategoryId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.ReadAsAsync<PagedResult<RecipeSearchResultDto>>()).Items);
    }

    /// <summary>A1: q rỗng hoặc &lt; 2 ký tự → 400 VALIDATION_ERROR.</summary>
    [Theory]
    [InlineData("/api/v1/recipes/search")]
    [InlineData("/api/v1/recipes/search?q=p")]
    [InlineData("/api/v1/recipes/search?q=pho&pageSize=51")]
    public async Task Search_InvalidQuery_Returns400(string url)
    {
        var response = await factory.CreateClient().GetAsync(new Uri(url, UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>A3: toán tử tsquery do người dùng tự gõ bị làm sạch — không lỗi cú pháp, không 500.</summary>
    [Theory]
    [InlineData("pho:* | !bo")]
    [InlineData("'; DROP TABLE \"Recipes\"; --")]
    [InlineData("!!")]
    public async Task Search_SpecialCharacters_Returns200(string q)
    {
        var response = await factory.CreateClient()
            .GetAsync(new Uri($"/api/v1/recipes/search?q={Uri.EscapeDataString(q)}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- FR-RCP-011 – GET /recipes/mine ----------
    [Fact]
    public async Task Mine_AsAuthor_ReturnsOwnRecipesInEveryStatus_NoStore()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .GetAsync(new Uri("/api/v1/recipes/mine", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore, "Endpoint riêng tư phải trả Cache-Control: no-store.");

        var page = await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        Assert.Equal(SeededRecipesOfAuthor, page.TotalCount);
        Assert.Contains(page.Items, r => r.Status == RecipeStatus.Draft);
    }

    [Fact]
    public async Task Mine_FilterByStatus_ReturnsOnlyThatStatus()
    {
        var page = await (await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
                .GetAsync(new Uri("/api/v1/recipes/mine?status=Draft", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSummaryDto>>();

        var draft = Assert.Single(page.Items);
        Assert.Equal(TestDataSeeder.DraftRecipeSlug, draft.Slug);
    }

    /// <summary>A4: chưa có công thức nào → 200 với items rỗng, không 404.</summary>
    [Fact]
    public async Task Mine_AuthorWithoutRecipes_Returns200Empty()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId)
            .GetAsync(new Uri("/api/v1/recipes/mine", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>()).Items);
    }

    /// <summary>A1: không có token → 401.</summary>
    [Fact]
    public async Task Mine_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/mine", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>A2: Author truyền authorId của người khác → 403 RECIPE_FORBIDDEN.</summary>
    [Fact]
    public async Task Mine_AuthorPassingAuthorId_Returns403()
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId)
            .GetAsync(new Uri($"/api/v1/recipes/mine?authorId={TestDataSeeder.AuthorUserId}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);
    }

    [Fact]
    public async Task Mine_AdminPassingAuthorId_ReturnsThatAuthorsRecipes()
    {
        var response = await factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId)
            .GetAsync(new Uri($"/api/v1/recipes/mine?authorId={TestDataSeeder.AuthorUserId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(SeededRecipesOfAuthor, (await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>()).TotalCount);
    }

    /// <summary>A3: sortBy, sortOrder hoặc status ngoài whitelist → 400 VALIDATION_ERROR.</summary>
    [Theory]
    [InlineData("?status=Deleted")]
    [InlineData("?sortBy=authorId")]
    [InlineData("?sortOrder=random")]
    public async Task Mine_InvalidQuery_Returns400(string query)
    {
        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .GetAsync(new Uri($"/api/v1/recipes/mine{query}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    // ---------- CR-2026-04 (d) – GET /recipes/mine/{id} ----------
    [Fact]
    public async Task MineById_OwnDraft_Returns200WithRowVersionETagAndNoStore()
    {
        var owner = factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId);
        var draftId = await DraftIdAsync(owner);

        var response = await owner.GetAsync(new Uri($"/api/v1/recipes/mine/{draftId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal(RecipeStatus.Draft, detail.Status);
        Assert.NotEmpty(detail.Steps);
        Assert.NotEmpty(detail.Ingredients);
        Assert.False(string.IsNullOrEmpty(detail.RowVersion));
        Assert.Equal($"\"{detail.RowVersion}\"", response.Headers.ETag?.Tag);
        Assert.True(response.Headers.CacheControl?.NoStore, "Endpoint riêng tư phải trả Cache-Control: no-store.");
    }

    [Fact]
    public async Task MineById_AsAdmin_Returns200()
    {
        var draftId = await DraftIdAsync(factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId));

        var response = await factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId)
            .GetAsync(new Uri($"/api/v1/recipes/mine/{draftId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>A5: của người khác và không tồn tại → CÙNG 404 RECIPE_NOT_FOUND (không 403 — không dò được id).</summary>
    [Fact]
    public async Task MineById_OtherAuthorsRecipeOrUnknownId_Returns404()
    {
        var draftId = await DraftIdAsync(factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId));
        var other = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);

        var foreign = await other.GetAsync(new Uri($"/api/v1/recipes/mine/{draftId}", UriKind.Relative));
        var unknown = await other.GetAsync(new Uri($"/api/v1/recipes/mine/{Guid.NewGuid()}", UriKind.Relative));

        await foreign.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
        await unknown.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
    }

    [Fact]
    public async Task MineById_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync(new Uri($"/api/v1/recipes/mine/{Guid.NewGuid()}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- MT-48 – GET /recipes/sitemap ----------
    [Fact]
    public async Task Sitemap_ContainsEveryPublishedSlugOnly()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/sitemap", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await response.ReadAsAsync<List<RecipeSitemapEntryDto>>();
        Assert.Equal(TestDataSeeder.PublishedRecipeCount, entries.Count);
        Assert.Contains(entries, e => e.Slug == TestDataSeeder.PublishedRecipeSlug);
        Assert.DoesNotContain(entries, e => e.Slug == TestDataSeeder.DraftRecipeSlug);
        Assert.All(entries, e => Assert.NotEqual(default, e.UpdatedAt));
    }

    /// <summary>Sitemap là endpoint công khai chỉ đọc: phương thức khác GET không được định tuyến (405).</summary>
    [Fact]
    public async Task Sitemap_Post_Returns405()
    {
        var response = await factory.CreateClient().PostAsync(new Uri("/api/v1/recipes/sitemap", UriKind.Relative), content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    private static async Task<Guid> DraftIdAsync(HttpClient owner)
    {
        var page = await (await owner.GetAsync(new Uri("/api/v1/recipes/mine?status=Draft", UriKind.Relative)))
            .ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        return Assert.Single(page.Items).Id;
    }
}
