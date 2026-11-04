using CulinaryBlog.Domain.Common;
using CulinaryBlog.Infrastructure.Persistence.Seed;

namespace CulinaryBlog.API.IntegrationTests.Seed;

/// <summary>SRS §2.6.1 (CR-2026-07): công thức bổ sung từ Wikibooks phải truy vết được nguồn, có ghi công và ảnh giấy phép mở.</summary>
public class SupplementedCatalogTests
{
    private static readonly string[] AllowedHosts = ["upload.wikimedia.org", "thumb.wikimedia.org"];

    private static IEnumerable<SeedRecipe> Wikibooks => RecipeSeedCatalog.Recipes.Where(r => r.Source is not null);

    private static IEnumerable<SeedRecipe> Legacy => RecipeSeedCatalog.Recipes.Where(r => r.Source is null);

    [Fact]
    public void LegacyRecipes_AreKept_AndStillMeetThresholdsOnTheirOwn()
    {
        Assert.Equal(100, Legacy.Count());
        Assert.Contains(Legacy, r => r.Title.StartsWith("Phở", StringComparison.Ordinal));
    }

    [Fact]
    public void NoLegacyRecipe_CarriesNutrition()
    {
        Assert.All(Legacy, r => Assert.Null(r.Nutrition));
    }

    [Fact]
    public void WikibooksRecipes_AreSupplemented_InVietnamese()
    {
        Assert.True(Wikibooks.Count() >= 100, $"Chỉ có {Wikibooks.Count()} công thức bổ sung.");
        Assert.All(Wikibooks, r =>
        {
            Assert.Matches("[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]", r.Description.ToLowerInvariant());
            Assert.All(r.Steps, s => Assert.InRange(s.Description.Length, 1, 2000));
            Assert.InRange(r.Title.Length, 5, 200);
            Assert.InRange(r.Description.Length, 20, 2000);
        });
    }

    [Fact]
    public void WikibooksRecipes_HaveCompleteAttribution()
    {
        Assert.All(Wikibooks, r =>
        {
            var note = r.Attribution ?? string.Empty;
            Assert.StartsWith(RecipeSeedCatalog.SourceNotePrefix, note, StringComparison.Ordinal);
            Assert.Contains($"curid={r.Source!.PageId}&oldid={r.Source.RevisionId}", note, StringComparison.Ordinal);
            Assert.Contains("CC BY-SA 4.0", note, StringComparison.Ordinal);
            Assert.Contains("https://creativecommons.org/licenses/by-sa/4.0/", note, StringComparison.Ordinal);
            Assert.Contains("có hỗ trợ AI", note, StringComparison.Ordinal);
            Assert.Contains(r.Image!.DescriptionUrl.AbsoluteUri, note, StringComparison.Ordinal);
            Assert.InRange(note.Length, 1, 5000);
        });
    }

    [Fact]
    public void EverySeedImage_HasOpenLicence_AndAllowedHost()
    {
        var images = Wikibooks.Select(r => r.Image!).Concat(RecipeSeedCatalog.LegacyImages.Values).ToList();

        Assert.All(images, image =>
        {
            Assert.Matches("(?i)^(cc0|cc[- ]?by(-sa)?[- ]\\d|public domain)", image.License);
            Assert.Contains(image.Url.Host, AllowedHosts);
            Assert.Equal(Uri.UriSchemeHttps, image.Url.Scheme);
        });
    }

    [Fact]
    public void LegacyImages_PointToExistingLegacyRecipes()
    {
        var legacySlugs = Legacy.Select(r => SlugHelper.Generate(r.Title)).ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(RecipeSeedCatalog.LegacyImages);
        Assert.All(RecipeSeedCatalog.LegacyImages.Keys, slug => Assert.Contains(slug, legacySlugs));
    }

    [Fact]
    public void Nutrition_ComesOnlyFromSource_AndIsPlausible()
    {
        var withNutrition = Wikibooks.Where(r => r.Nutrition is not null).ToList();

        Assert.NotEmpty(withNutrition);
        Assert.All(withNutrition, r =>
        {
            var n = r.Nutrition!;
            var estimate = (4 * n.Protein!.Value) + (4 * n.Carbohydrates!.Value) + (9 * n.Fat!.Value);
            Assert.True(Math.Abs(estimate - n.Calories!.Value) / n.Calories.Value <= 0.35m, $"{r.Title}: {n.Calories} kcal, ước tính {estimate}");
        });
    }

    [Fact]
    public void SlugHelper_MatchesEtlSlugs_ForLegacyImages()
    {
        Assert.Contains(SlugHelper.Generate("Phở bò Hà Nội"), RecipeSeedCatalog.LegacyImages.Keys);
    }
}
