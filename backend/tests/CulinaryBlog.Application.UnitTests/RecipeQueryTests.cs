using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using NSubstitute;

namespace CulinaryBlog.Application.UnitTests;

/// <summary>
/// Buổi 4 — Dev 3: quy tắc phía Application của các truy vấn công thức — whitelist sắp xếp (FR-SRCH-003, D-10), dựng tsquery
/// (FR-SRCH-001), khóa cache (NFR-PERF-003) và phân quyền authorId của /recipes/mine (FR-RCP-011).
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

    // ---------- Search (FR-SRCH-001) ----------
    [Theory]
    [InlineData("pho bo", "pho:* & bo:*")]
    [InlineData("Phở  Bò", "pho:* & bo:*")]
    [InlineData("đậu (xanh) & !", "dau:* & xanh:*")]
    [InlineData("pho:* | bo", "pho:* & bo:*")]
    [InlineData("!!", "")]
    public void SearchTermBuilder_UnaccentsAndSanitizes(string input, string expected) =>
        Assert.Equal(expected, SearchTermBuilder.Build(input));

    [Theory]
    [InlineData("p", false)]
    [InlineData(" p ", false)]
    [InlineData("", false)]
    [InlineData("ph", true)]
    public void Search_TermMinLength2(string q, bool valid) =>
        Assert.Equal(valid, new SearchRecipesQueryValidator().Validate(new SearchRecipesQuery(q)).IsValid);

    [Fact]
    public void Search_CacheKey_SameForAccentedAndPlain() =>
        Assert.Equal(new SearchRecipesQuery("Phở bò").CacheKey, new SearchRecipesQuery("pho  bo").CacheKey);

    [Fact]
    public async Task Search_OnlySpecialCharacters_ReturnsEmptyPageWithoutQueryingDatabase()
    {
        var repository = Substitute.For<IRecipeReadRepository>();

        var page = await new SearchRecipesQueryHandler(repository).Handle(new SearchRecipesQuery("!!"), CancellationToken.None);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
        await repository.DidNotReceiveWithAnyArgs()
            .SearchPublishedAsync(default!, default, default, default!, default);
    }

    // ---------- GetMyRecipesQuery (FR-RCP-011) ----------
    [Fact]
    public void Mine_IsNeverCached() => Assert.False(typeof(ICacheable).IsAssignableFrom(typeof(GetMyRecipesQuery)));

    [Fact]
    public void MineById_IsNeverCached() => Assert.False(typeof(ICacheable).IsAssignableFrom(typeof(GetMyRecipeByIdQuery)));

    [Fact]
    public void Mine_StatusOutsideEnum_Fails() =>
        Assert.False(new GetMyRecipesQueryValidator().Validate(new GetMyRecipesQuery(Status: (RecipeStatus)7)).IsValid);

    [Fact]
    public async Task Mine_AuthorPassingAuthorId_Throws403()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("22222222-2222-2222-2222-222222222222");
        currentUser.IsAdmin.Returns(false);
        var handler = new GetMyRecipesQueryHandler(Substitute.For<IRecipeReadRepository>(), currentUser);

        var error = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new GetMyRecipesQuery(AuthorId: Guid.Parse("33333333-3333-3333-3333-333333333333")), CancellationToken.None));

        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    public async Task Mine_AdminPassingAuthorId_QueriesThatAuthor()
    {
        const string otherAuthor = "33333333-3333-3333-3333-333333333333";
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("11111111-1111-1111-1111-111111111111");
        currentUser.IsAdmin.Returns(true);
        var repository = Substitute.For<IRecipeReadRepository>();
        repository.GetByAuthorPagedAsync(default!, default, default!, default)
            .ReturnsForAnyArgs(new PagedResult<RecipeSummaryDto>([], 0, 1, 12));

        await new GetMyRecipesQueryHandler(repository, currentUser)
            .Handle(new GetMyRecipesQuery(AuthorId: Guid.Parse(otherAuthor)), CancellationToken.None);

        await repository.Received(1).GetByAuthorPagedAsync(
            Arg.Is(otherAuthor),
            Arg.Is<RecipeStatus?>(status => !status.HasValue),
            Arg.Any<RecipeListCriteria>(),
            Arg.Any<CancellationToken>());
    }
}
