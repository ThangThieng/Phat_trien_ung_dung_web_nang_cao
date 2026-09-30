using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.UnitTests;

/// <summary>
/// Buổi 4 — Dev 3 (phần vá D-10/D-6): quy tắc phía Application của truy vấn danh sách công thức — whitelist sắp xếp
/// (FR-SRCH-003), bộ lọc (FR-SRCH-002), phân trang (FR-SRCH-004) và khóa cache (NFR-PERF-003).
/// </summary>
public class RecipeQueryTests
{
    // ---------- SortMapper (FR-SRCH-003 / MT-01) ----------
    [Theory]
    [InlineData("createdAt")]
    [InlineData("publishedAt")]
    [InlineData("title")]
    [InlineData("cookTime")]
    [InlineData("prepTime")]
    [InlineData("TITLE")]
    [InlineData(null)]
    public void SortMapper_AcceptsWhitelist(string? sortBy) => Assert.True(SortMapper.IsValidSortBy(sortBy));

    [Theory]
    [InlineData("password")]
    [InlineData("-createdAt")]
    [InlineData("servings")]
    [InlineData("")]
    public void SortMapper_RejectsOutsideWhitelist(string sortBy) => Assert.False(SortMapper.IsValidSortBy(sortBy));

    [Theory]
    [InlineData(null, true)]
    [InlineData("asc", true)]
    [InlineData("DESC", true)]
    [InlineData("up", false)]
    public void SortMapper_SortOrder(string? sortOrder, bool valid) => Assert.Equal(valid, SortMapper.IsValidSortOrder(sortOrder));

    [Fact]
    public void SortMapper_Default_IsCreatedAtDescending()
    {
        var sort = SortMapper.Map(null, null);

        Assert.True(sort.Descending);
        Assert.Contains("CreatedAt", sort.KeySelector.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void SortMapper_Asc_IsAscending() => Assert.False(SortMapper.Map("title", "asc").Descending);

    // ---------- GetRecipesQuery validator (FR-RCP-001 / FR-SRCH-002/003/004) ----------
    [Fact]
    public void GetRecipes_DefaultPageSize_Is12() => Assert.Equal(12, new GetRecipesQuery().PageSize);

    [Fact]
    public void GetRecipes_LegacySortParameter_Fails()
    {
        var result = new GetRecipesQueryValidator().Validate(new GetRecipesQuery(LegacySort: "-createdAt"));

        Assert.Contains(result.Errors, e => e.PropertyName == "sort");
    }

    [Fact]
    public void GetRecipes_DifficultyOutsideEnum_Fails() =>
        Assert.False(new GetRecipesQueryValidator().Validate(new GetRecipesQuery(Difficulty: (RecipeDifficulty)9)).IsValid);

    [Fact]
    public void GetRecipes_NegativeFilters_Fail()
    {
        var result = new GetRecipesQueryValidator().Validate(new GetRecipesQuery(MaxCookTime: -1, MaxPrepTime: -1, MinServings: 0));

        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void GetRecipes_CacheKey_IgnoresSortCaseAndHasListPrefix()
    {
        var lower = new GetRecipesQuery(SortBy: "title", SortOrder: "asc").CacheKey;
        var upper = new GetRecipesQuery(SortBy: "TITLE", SortOrder: "ASC").CacheKey;

        Assert.Equal(lower, upper);
        Assert.StartsWith(RecipeCacheKeys.ListPrefix, lower, StringComparison.Ordinal);
        Assert.NotEqual(lower, new GetRecipesQuery(SortBy: "title", SortOrder: "desc").CacheKey);
    }

    [Fact]
    public void GetCategoryBySlug_CacheKey_HasDetailPrefixAndSlug() =>
        Assert.StartsWith($"{CategoryCacheKeys.DetailPrefix}mon-chinh:", new GetCategoryBySlugQuery("mon-chinh").CacheKey, StringComparison.Ordinal);
}
