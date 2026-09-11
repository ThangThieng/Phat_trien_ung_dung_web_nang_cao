using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.UnitTests;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Phở bò Hà Nội", "pho-bo-ha-noi")]
    [InlineData("Đậu hũ sốt cà chua", "dau-hu-sot-ca-chua")]
    [InlineData("  Canh & Súp  ", "canh-sup")]
    [InlineData("Bún---chả!!", "bun-cha")]
    [InlineData("Tráng miệng & Đồ uống", "trang-mieng-do-uong")]
    public void Generate_RemovesVietnameseDiacritics_AndNormalizes(string input, string expected) =>
        Assert.Equal(expected, SlugHelper.Generate(input));

    [Theory]
    [InlineData(1, "pho-bo")]
    [InlineData(2, "pho-bo-2")]
    [InlineData(10, "pho-bo-10")]
    public void WithSuffix_AppendsNumber_FromSecondOccurrence(int number, string expected) =>
        Assert.Equal(expected, SlugHelper.WithSuffix("pho-bo", number));
}
