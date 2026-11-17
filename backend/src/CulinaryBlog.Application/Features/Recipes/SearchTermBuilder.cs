using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-SRCH-001 bước 3 + A3 — dựng biểu thức <c>tsquery</c> từ chuỗi người dùng nhập: "Phở  bò!" → <c>pho:* &amp; bo:*</c>.
/// Bỏ dấu tiếng Việt và hạ chữ thường cho khớp vế <c>SearchVector</c> (<c>to_tsvector('simple', unaccent_immutable(...))</c>),
/// dùng lại đúng quy tắc của <see cref="SlugHelper"/> (lower → đ→d → bỏ dấu) để hai nơi không lệch nhau. Làm sạch: mọi ký tự
/// không phải chữ/số (gồm <c>&amp; | ! ( ) : * ' &lt; &gt;</c>) bị bỏ, nên chuỗi đưa vào <c>to_tsquery</c> không thể mang toán tử do
/// người dùng tự viết — vừa chống lỗi cú pháp, vừa chống injection; giá trị vẫn đi qua tham số của EF Core, không ghép SQL.
/// Mỗi từ thành tiền tố (<c>:*</c>) và các từ nối bằng AND (<c>&amp;</c>).
/// </summary>
public static class SearchTermBuilder
{
    /// <summary>
    /// CR-2026-08 / MT-71: từ ghép và token do hàm <c>search_normalize</c> (migration B8_Search_TitleWeightCompoundTerms) sinh ra.
    /// Hai danh sách phải khớp nhau: <c>RecipeSearchRelevanceTests</c> kiểm từng cặp trên PostgreSQL thật.
    /// </summary>
    private static readonly Dictionary<string, string> CompoundTokens = new(StringComparer.Ordinal)
    {
        ["pho mai"] = "qphomai",
        ["pho mat"] = "qphomai",
        ["pho bien"] = "qphobien",
        ["pho thong"] = "qphothong",
        ["thanh pho"] = "qthanhpho",
        ["duong pho"] = "qduongpho",
        ["khu pho"] = "qkhupho",
        ["pho co"] = "qphoco",
        ["pho hoi"] = "qphohoi",
        ["phomai"] = "qphomai",
        ["phomat"] = "qphomai",
    };

    /// <summary>Trả chuỗi rỗng khi không còn từ nào sau khi làm sạch (ví dụ <c>q = "!!"</c>) — handler trả trang rỗng.</summary>
    public static string Build(string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);

        var words = SlugHelper.Generate(searchTerm).Split('-', StringSplitOptions.RemoveEmptyEntries);
        var terms = MergeCompounds(words).Distinct(StringComparer.Ordinal).Select(term => term + ":*");

        return string.Join(" & ", terms);
    }

    private static IEnumerable<string> MergeCompounds(string[] words)
    {
        var i = 0;
        while (i < words.Length)
        {
            if (i + 1 < words.Length && CompoundTokens.TryGetValue(words[i] + " " + words[i + 1], out var pair))
            {
                yield return pair;
                i += 2;
            }
            else
            {
                yield return CompoundTokens.GetValueOrDefault(words[i], words[i]);
                i++;
            }
        }
    }
}
