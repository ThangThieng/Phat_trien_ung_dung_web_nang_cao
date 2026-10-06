namespace CulinaryBlog.Domain.Common;

/// <summary>
/// NFR-SEO-004 (MT-53) – danh sách slug dành riêng, dùng chung cho cả Recipe lẫn Category.
/// ASP.NET Core ưu tiên segment literal hơn tham số {slug} bất kể thứ tự khai báo, nên một bản ghi
/// mang slug trùng các giá trị này sẽ KHÔNG BAO GIỜ truy cập được → phải chặn ngay lúc sinh slug.
/// Đây là nguồn duy nhất của danh sách; không hardcode lại mảng string ở nơi khác.
/// </summary>
public static class ReservedSlugs
{
    private static readonly HashSet<string> Values = new(StringComparer.OrdinalIgnoreCase)
    {
        "search",
        "mine",
        "sitemap",
        "new",
        "edit",
    };

    public static bool Contains(string slug) => Values.Contains(slug);
}
