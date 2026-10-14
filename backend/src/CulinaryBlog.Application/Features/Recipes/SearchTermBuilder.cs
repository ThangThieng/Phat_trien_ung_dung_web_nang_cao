using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>Chuẩn hóa từ khóa không dấu và chỉ giữ các token chữ/số trước khi tạo tsquery.</summary>
public static class SearchTermBuilder
{
    public static string Build(string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);
        return string.Join(" & ", SlugHelper.Generate(searchTerm)
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .Select(term => term + ":*"));
    }
}
