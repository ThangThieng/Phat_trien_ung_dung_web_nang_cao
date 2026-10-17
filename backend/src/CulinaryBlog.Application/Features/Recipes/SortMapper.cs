using System.Linq.Expressions;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>Cách sắp xếp đã được ánh xạ từ cặp <c>sortBy</c> + <c>sortOrder</c> (FR-SRCH-003).</summary>
public sealed record RecipeSort(Expression<Func<Recipe, object?>> KeySelector, bool Descending);

/// <summary>
/// FR-SRCH-003 / MT-01 — whitelist sắp xếp dùng chung cho mọi truy vấn danh sách công thức (FR-RCP-001, FR-CAT-002,
/// FR-RCP-011). <c>sortBy</c> được ánh xạ sang biểu thức cột qua dictionary ở tầng Application — KHÔNG BAO GIỜ ghép chuỗi
/// SQL — nên whitelist này đồng thời là rào chắn chống SQL injection qua tên cột. Thay cho <c>RecipeSortParser</c> một tham số
/// <c>sort=-field</c> của Buổi 2 (retrofit D-10).
/// </summary>
public static class SortMapper
{
    public const string DefaultSortBy = "createdAt";

    public const string DefaultSortOrder = "desc";

    public const string SortOrderMessage = "sortOrder chỉ chấp nhận: asc, desc.";

    private const string Ascending = "asc";

    private static readonly Dictionary<string, Expression<Func<Recipe, object?>>> Columns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdAt"] = r => r.CreatedAt,
        ["publishedAt"] = r => r.PublishedAt,
        ["title"] = r => r.Title,
        ["cookTime"] = r => r.CookTimeMinutes,
        ["prepTime"] = r => r.PrepTimeMinutes,
    };

    /// <summary>Thông điệp lỗi dùng chung của validator (liệt kê đúng 5 giá trị hợp lệ).</summary>
    public static string SortByMessage { get; } =
        $"sortBy chỉ chấp nhận: {string.Join(", ", Columns.Keys)}.";

    /// <summary>Bỏ trống = mặc định <c>createdAt</c>; có giá trị thì phải nằm trong whitelist.</summary>
    public static bool IsValidSortBy(string? sortBy) => sortBy is null || Columns.ContainsKey(sortBy);

    public static bool IsValidSortOrder(string? sortOrder) =>
        sortOrder is null
        || string.Equals(sortOrder, Ascending, StringComparison.OrdinalIgnoreCase)
        || string.Equals(sortOrder, DefaultSortOrder, StringComparison.OrdinalIgnoreCase);

    /// <summary>Chỉ gọi SAU khi validator đã chạy — giá trị ngoài whitelist ở đây là lỗi lập trình, không phải lỗi người dùng.</summary>
    public static RecipeSort Map(string? sortBy, string? sortOrder)
    {
        var key = sortBy ?? DefaultSortBy;
        if (!Columns.TryGetValue(key, out var selector))
        {
            throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, SortByMessage);
        }

        var descending = !string.Equals(sortOrder ?? DefaultSortOrder, Ascending, StringComparison.OrdinalIgnoreCase);
        return new RecipeSort(selector, descending);
    }
}
