using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CulinaryBlog.Domain.Common;

/// <summary>Sinh slug URL-friendly: chữ thường, bỏ dấu tiếng Việt, khoảng trắng → "-" (FR-CAT-003, NFR-SEO-004).</summary>
public static partial class SlugHelper
{
    public static string Generate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var normalized = text.Trim().ToLowerInvariant()
            .Replace('đ', 'd')
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        var slug = NonAlphanumeric().Replace(builder.ToString(), "-");
        return MultipleDashes().Replace(slug, "-").Trim('-');
    }

    /// <summary>Thêm hậu tố số khi slug đã tồn tại: "pho-bo" → "pho-bo-2".</summary>
    public static string WithSuffix(string slug, int number) =>
        number <= 1 ? slug : $"{slug}-{number.ToString(CultureInfo.InvariantCulture)}";

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleDashes();
}
