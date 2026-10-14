using System.Net;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>Ba endpoint còn thiếu trong SRS §8.3; chạy với PostgreSQL/Redis thật qua Testcontainers.</summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeDiscoveryEndpointsTests(CulinaryBlogApiFactory factory)
{
    [Fact]
    public async Task Search_UnaccentedQuery_FindsPublishedRecipeOnly()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/search?q=pho", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.ReadAsAsync<PagedResult<RecipeSearchResultDto>>();
        Assert.Contains(page.Items, item => item.Slug == TestDataSeeder.PublishedRecipeSlug && item.RelevanceScore > 0);
        Assert.All(page.Items, item => Assert.Equal(RecipeStatus.Published, item.Status));
    }

    [Fact]
    public async Task Search_TooShort_ReturnsBadRequest()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/search?q=p", UriKind.Relative));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Sitemap_ContainsPublishedSlugsButNotDrafts()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/sitemap", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await response.ReadAsAsync<List<RecipeSitemapEntryDto>>();
        Assert.Equal(TestDataSeeder.PublishedRecipeCount, entries.Count);
        Assert.DoesNotContain(entries, item => item.Slug == TestDataSeeder.DraftRecipeSlug);
    }

    [Fact]
    public async Task Mine_IsPrivateNoStoreAndIncludesDraft()
    {
        var unauthorized = await factory.CreateClient().GetAsync(new Uri("/api/v1/recipes/mine", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var response = await factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId)
            .GetAsync(new Uri("/api/v1/recipes/mine?status=Draft", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var page = await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        Assert.Equal(TestDataSeeder.DraftRecipeSlug, Assert.Single(page.Items).Slug);
    }

    [Fact]
    public async Task Mine_AuthorIdFilter_IsAdminOnly()
    {
        var path = new Uri($"/api/v1/recipes/mine?authorId={TestDataSeeder.AuthorUserId}", UriKind.Relative);
        var forbidden = await factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId).GetAsync(path);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var allowed = await factory.CreateClientAs("Author,Admin", TestDataSeeder.AdminUserId).GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        var page = await allowed.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        Assert.Contains(page.Items, item => item.Slug == TestDataSeeder.DraftRecipeSlug);
    }
}
