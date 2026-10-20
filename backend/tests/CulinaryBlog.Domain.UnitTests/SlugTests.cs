using CulinaryBlog.Domain.ValueObjects;

namespace CulinaryBlog.Domain.UnitTests;

/// <summary>Value Object <c>Slug</c> (SRS §6.2): định dạng hợp lệ, sinh từ tiếng Việt, hậu tố, slug dành riêng.</summary>
public class SlugTests
{
    [Theory]
    [InlineData("pho-bo")]
    [InlineData("mon-chinh-2")]
    [InlineData("a")]
    [InlineData("123")]
    public void From_WellFormedValue_IsAccepted(string value) =>
        Assert.Equal(value, Slug.From(value).Value);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Pho-bo")]
    [InlineData("phở-bò")]
    [InlineData("pho bo")]
    [InlineData("-pho-bo")]
    [InlineData("pho-bo-")]
    [InlineData("pho--bo")]
    [InlineData("pho_bo")]
    public void From_MalformedValue_Throws(string value) =>
        Assert.Throws<ArgumentException>(() => Slug.From(value));

    [Fact]
    public void From_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => Slug.From(null!));

    [Theory]
    [InlineData("Phở bò Hà Nội", "pho-bo-ha-noi")]
    [InlineData("Tráng miệng & Đồ uống", "trang-mieng-do-uong")]
    [InlineData("Món-chay", "mon-chay")]
    public void FromText_RemovesVietnameseDiacritics(string text, string expected) =>
        Assert.Equal(expected, Slug.FromText(text).Value);

    [Theory]
    [InlineData("!!!")]
    [InlineData("---")]
    public void FromText_WithoutLettersOrDigits_Throws(string text) =>
        Assert.Throws<ArgumentException>(() => Slug.FromText(text));

    [Theory]
    [InlineData(1, "pho-bo")]
    [InlineData(2, "pho-bo-2")]
    [InlineData(10, "pho-bo-10")]
    public void WithSuffix_AppendsNumberFromSecondOccurrence(int number, string expected) =>
        Assert.Equal(expected, Slug.From("pho-bo").WithSuffix(number).Value);

    [Theory]
    [InlineData("search", true)]
    [InlineData("mine", true)]
    [InlineData("sitemap", true)]
    [InlineData("new", true)]
    [InlineData("edit", true)]
    [InlineData("search-2", false)]
    [InlineData("pho-bo", false)]
    public void IsReserved_FollowsReservedSlugList(string value, bool expected) =>
        Assert.Equal(expected, Slug.From(value).IsReserved);

    [Fact]
    public void Slugs_WithSameValue_AreEqual() =>
        Assert.Equal(Slug.From("pho-bo"), Slug.FromText("Phở bò"));
}
