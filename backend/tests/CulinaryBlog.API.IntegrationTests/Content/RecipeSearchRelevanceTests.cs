using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>CR-2026-08 / MT-71: trên PostgreSQL thật, vì biểu thức nằm trong cột generated và hàm <c>search_normalize</c>.</summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeSearchRelevanceTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task TitleMatch_RanksAbove_RepeatedDescriptionMatch()
    {
        await AddPublishedAsync("Bún trộn", "bun-tron-pho", "Ăn thay phở, ngon như phở, rẻ hơn phở và dễ hơn phở.");
        await AddPublishedAsync("Phở gà", "pho-ga-test", "Nước dùng trong.");

        var page = await SearchAsync("pho");

        Assert.Equal("pho-ga-test", page.Items.First(r => r.Slug is "pho-ga-test" or "bun-tron-pho").Slug);
        var title = page.Items.Single(r => r.Slug == "pho-ga-test").RelevanceScore;
        var description = page.Items.Single(r => r.Slug == "bun-tron-pho").RelevanceScore;
        Assert.True(title > description, $"{title} <= {description}");
    }

    [Theory]
    [InlineData("Bánh phủ phô mai nướng.", "pho mai")]
    [InlineData("Phô mai tan chảy.", "phô mai")]
    [InlineData("Rắc phô-mai lên trên.", "phomai")]
    [InlineData("Thêm phô mát.", "pho mat")]
    [InlineData("Món phổ biến ở miền Bắc.", "pho bien")]
    [InlineData("Kiến thức phổ thông.", "pho thong")]
    [InlineData("Nổi tiếng ở thành phố lớn.", "thanh pho")]
    [InlineData("Món đường phố.", "duong pho")]
    [InlineData("Quán trong khu phố.", "khu pho")]
    [InlineData("Quán ở phố cổ.", "pho co")]
    [InlineData("Đặc sản phố Hội.", "pho hoi")]
    public async Task CompoundWord_DoesNotMatchPho_ButIsFoundByItsOwnWords(string description, string query)
    {
        await AddPublishedAsync("Món thử", "mon-thu-tu-ghep", description);

        Assert.DoesNotContain((await SearchAsync("pho")).Items, r => r.Slug == "mon-thu-tu-ghep");
        Assert.Contains((await SearchAsync(query)).Items, r => r.Slug == "mon-thu-tu-ghep");
    }

    [Theory]
    [InlineData("Phở biển nước dùng ngọt.")]
    [InlineData("Một tô phở, mai ăn tiếp.")]
    public async Task RealPho_IsNeverMergedIntoACompound(string description)
    {
        await AddPublishedAsync("Món thử", "mon-thu-pho-that", description);

        Assert.Contains((await SearchAsync("pho")).Items, r => r.Slug == "mon-thu-pho-that");
    }

    private async Task<PagedResult<RecipeSearchResultDto>> SearchAsync(string query)
    {
        var response = await factory.CreateClient()
            .GetAsync(new Uri($"/api/v1/recipes/search?pageSize=50&q={Uri.EscapeDataString(query)}", UriKind.Relative));
        return await response.ReadAsAsync<PagedResult<RecipeSearchResultDto>>();
    }

    private async Task AddPublishedAsync(string title, string slug, string description)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var recipe = Recipe.Create(
            title,
            slug,
            description,
            TestDataSeeder.MonChinhCategoryId,
            TestDataSeeder.AuthorUserId,
            10,
            10,
            2,
            RecipeDifficulty.Easy);
        recipe.AddIngredient(new IngredientDetails("Nguyên liệu", 1, null, "g", null));
        recipe.AddStep("Nấu", "Nấu chín.");
        recipe.Publish(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
    }
}
