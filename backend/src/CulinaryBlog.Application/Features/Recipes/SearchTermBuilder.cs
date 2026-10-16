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
    /// <summary>Trả chuỗi rỗng khi không còn từ nào sau khi làm sạch (ví dụ <c>q = "!!"</c>) — handler trả trang rỗng.</summary>
    public static string Build(string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);

        var terms = SlugHelper.Generate(searchTerm)
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .Select(term => term + ":*");

        return string.Join(" & ", terms);
    }
}
