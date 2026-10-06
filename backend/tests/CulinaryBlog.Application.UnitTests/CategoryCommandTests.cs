using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Exceptions.Categories;
using NSubstitute;

namespace CulinaryBlog.Application.UnitTests;

/// <summary>
/// FR-CAT-003/004/005 – unit test validator + handler với repository giả lập (NSubstitute), không cần Docker.
/// Luồng qua HTTP thật (201/403/409/204, race hai request song song) nằm ở <c>CategoryAdminEndpointsTests</c>
/// của project API.IntegrationTests. Mã HTTP của từng exception được kiểm ở <c>CategoryExceptionMappingsTests</c>.
/// </summary>
public class CategoryCommandTests
{
    private readonly CreateCategoryCommandValidator _createValidator = new();
    private readonly UpdateCategoryCommandValidator _updateValidator = new();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly ICategoryReadRepository _readCategories = Substitute.For<ICategoryReadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public CategoryCommandTests() => _unitOfWork.Categories.Returns(_categories);

    // ---------- Validator ----------
    [Fact]
    public void Create_ValidCommand_Passes() =>
        Assert.True(_createValidator.Validate(NewCommand("Món chính")).IsValid);

    /// <summary>MT-37: giới hạn chuẩn là 2–100 (§7.9), KHÔNG phải 2–50 như tài liệu cũ – tên 60 ký tự phải hợp lệ.</summary>
    [Theory]
    [InlineData(2, true)]
    [InlineData(60, true)]
    [InlineData(100, true)]
    [InlineData(1, false)]
    [InlineData(101, false)]
    public void Create_NameLength_FollowsStandardLimits(int length, bool valid) =>
        Assert.Equal(valid, _createValidator.Validate(NewCommand(new string('a', length))).IsValid);

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("Món <b>chính")]
    public void Create_NameWithHtml_Fails(string name) =>
        Assert.False(_createValidator.Validate(NewCommand(name)).IsValid);

    /// <summary>Tên chỉ gồm ký tự đặc biệt sinh ra slug rỗng → phải chặn ở validator (400) thay vì để Category.Create ném lỗi (500).</summary>
    [Theory]
    [InlineData("!!!")]
    [InlineData("---")]
    public void Create_NameWithoutUsableSlug_Fails(string name) =>
        Assert.False(_createValidator.Validate(NewCommand(name)).IsValid);

    [Fact]
    public void Create_DescriptionTooLong_Fails() =>
        Assert.False(_createValidator.Validate(NewCommand("Món chính", new string('a', 2001))).IsValid);

    [Theory]
    [InlineData(null, true)]
    [InlineData("https://cdn.example.com/a.png", true)]
    [InlineData("không-phải-url", false)]
    public void Create_ImageUrl_MustBeAbsoluteOrEmpty(string? imageUrl, bool valid) =>
        Assert.Equal(valid, _createValidator.Validate(NewCommand("Món chính", null, imageUrl)).IsValid);

    [Fact]
    public void Create_NegativeOrderIndex_Fails() =>
        Assert.False(_createValidator.Validate(new CreateCategoryCommand("Món chính", null, null, -1)).IsValid);

    [Fact]
    public void Update_MissingId_Fails() =>
        Assert.False(_updateValidator.Validate(new UpdateCategoryCommand(Guid.Empty, "Món chính", null, null)).IsValid);

    [Fact]
    public void Commands_InvalidateCategoryListAndDetailCache()
    {
        var commands = new ICacheInvalidator[]
        {
            NewCommand("Món chính"),
            new UpdateCategoryCommand(Guid.NewGuid(), "Món chính", null, null),
            new DeleteCategoryCommand(Guid.NewGuid()),
        };

        Assert.All(commands, command =>
        {
            Assert.Contains(CategoryCacheKeys.All, command.CacheKeysToInvalidate);
            Assert.Contains(CategoryCacheKeys.DetailPrefix, command.CacheKeysToInvalidate);
        });
    }

    /// <summary>FR-CAT-003 Create – MT-36: trùng Name phải bị chặn CHỦ ĐỘNG ở Application → 409, không để rơi xuống UNIQUE của DB (500).</summary>
    [Fact]
    public async Task Create_DuplicateName_Throws409()
    {
        _categories.ExistsByNameAsync("Món chính", null, Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(
            () => new CreateCategoryCommandHandler(_unitOfWork)
                .Handle(NewCommand("Món chính"), CancellationToken.None));

        Assert.Equal(ErrorCodes.CategoryNameExists, error.Code);
        await _categories.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_NewName_GeneratesSlugAndSaves()
    {
        StubSlugs();

        var result = await new CreateCategoryCommandHandler(_unitOfWork)
            .Handle(NewCommand("Món Chính"), CancellationToken.None);

        Assert.Equal("mon-chinh", result.Slug);
        Assert.Equal("Món Chính", result.Name);
        Assert.Equal(0, result.RecipeCount);
        await _categories.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_SlugTaken_AppendsSuffix()
    {
        StubSlugs("mon-chinh", "mon-chinh-2");

        var result = await new CreateCategoryCommandHandler(_unitOfWork)
            .Handle(NewCommand("Món chính"), CancellationToken.None);

        Assert.Equal("mon-chinh-3", result.Slug);
    }

    /// <summary>NFR-SEO-004 / MT-53: slug trùng từ khóa dành riêng sẽ không bao giờ truy cập được → phải thêm hậu tố.</summary>
    [Theory]
    [InlineData("Search", "search-2")]
    [InlineData("Sitemap", "sitemap-2")]
    [InlineData("New", "new-2")]
    public async Task Create_ReservedSlug_AppendsSuffix(string name, string expectedSlug)
    {
        StubSlugs();

        var result = await new CreateCategoryCommandHandler(_unitOfWork)
            .Handle(NewCommand(name), CancellationToken.None);

        Assert.Equal(expectedSlug, result.Slug);
    }

    // ---------- FR-CAT-004 Update ----------
    [Fact]
    public async Task Update_CategoryNotFound_Throws404()
    {
        _categories.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var error = await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => NewUpdateHandler().Handle(new UpdateCategoryCommand(Guid.NewGuid(), "Món chính", null, null), CancellationToken.None));

        Assert.Equal(ErrorCodes.CategoryNotFound, error.Code);
    }

    /// <summary>MT-36 áp dụng cho cả FR-CAT-004: đổi sang tên của danh mục KHÁC → 409 (excludeId bỏ qua chính nó).</summary>
    [Fact]
    public async Task Update_NameTakenByAnotherCategory_Throws409()
    {
        var category = Existing("Món chính", "mon-chinh");
        _categories.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _categories.ExistsByNameAsync("Món tráng miệng", category.Id, Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(
            () => NewUpdateHandler().Handle(new UpdateCategoryCommand(category.Id, "Món tráng miệng", null, null), CancellationToken.None));

        Assert.Equal(ErrorCodes.CategoryNameExists, error.Code);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>NFR-SEO-004: đổi Name KHÔNG được đổi Slug – link đã chia sẻ / đã lập chỉ mục phải còn sống.</summary>
    [Fact]
    public async Task Update_RenamesWithoutChangingSlug()
    {
        var category = Existing("Món chính", "mon-chinh");
        _categories.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _categories.ExistsByNameAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        _readCategories.GetBySlugAsync("mon-chinh", Arg.Any<CancellationToken>())
            .Returns(new CategoryDto(category.Id, "Món mặn", "mon-chinh", "Mô tả mới", null, 3, 7));

        var result = await NewUpdateHandler()
            .Handle(new UpdateCategoryCommand(category.Id, "Món mặn", "Mô tả mới", null, 3), CancellationToken.None);

        Assert.Equal("mon-chinh", category.Slug);
        Assert.Equal("Món mặn", category.Name);
        Assert.Equal("Mô tả mới", category.Description);
        Assert.Equal(3, category.OrderIndex);
        Assert.Equal("mon-chinh", result.Slug);
        Assert.Equal(7, result.RecipeCount);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ---------- FR-CAT-005 Delete ----------
    [Fact]
    public async Task Delete_CategoryNotFound_Throws404()
    {
        _categories.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var error = await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => new DeleteCategoryCommandHandler(_unitOfWork)
                .Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(ErrorCodes.CategoryNotFound, error.Code);
    }

    /// <summary>FR-CAT-005: còn công thức → 409 kèm SỐ LƯỢNG (trong detail và trong extension recipeCount) để Admin biết quy mô cần chuyển.</summary>
    [Fact]
    public async Task Delete_CategoryHasRecipes_Throws409WithCount()
    {
        var category = Existing("Món chính", "mon-chinh");
        _categories.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _categories.CountActiveRecipesAsync(category.Id, Arg.Any<CancellationToken>()).Returns(7);

        var error = await Assert.ThrowsAsync<CategoryHasRecipesException>(
            () => new DeleteCategoryCommandHandler(_unitOfWork)
                .Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None));

        Assert.Equal(ErrorCodes.CategoryDeleteHasRecipes, error.Code);
        Assert.Equal(7, error.RecipeCount);
        Assert.Equal(7, error.Extensions["recipeCount"]);
        Assert.Contains("7", error.Message);
        Assert.False(category.IsDeleted);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>MT-05/MT-20.11: xóa là SOFT DELETE – đánh dấu IsDeleted, không xóa vật lý.</summary>
    [Fact]
    public async Task Delete_EmptyCategory_SoftDeletes()
    {
        var category = Existing("Món chính", "mon-chinh");
        _categories.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _categories.CountActiveRecipesAsync(category.Id, Arg.Any<CancellationToken>()).Returns(0);

        await new DeleteCategoryCommandHandler(_unitOfWork)
            .Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.True(category.IsDeleted);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ---------- Helpers ----------
    private static CreateCategoryCommand NewCommand(string name, string? description = null, string? imageUrl = null) =>
        new(name, description, imageUrl);

    private static Category Existing(string name, string slug) => Category.Create(name, slug);

    private UpdateCategoryCommandHandler NewUpdateHandler() => new(_unitOfWork, _readCategories);

    private void StubSlugs(params string[] taken)
    {
        _categories.ExistsByNameAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        _categories.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => taken.Contains(call.Arg<string>()));
    }
}
