namespace CulinaryBlog.Application.Features.Recipes;

public static class RecipeSortParser
{
    /// <summary>Mặc định "-createdAt" (FR-SRCH-003).</summary>
    public static bool TryParse(string? sort, out RecipeSortField field, out bool descending)
    {
        field = RecipeSortField.CreatedAt;
        descending = true;

        if (string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }

        var value = sort.Trim();
        descending = value.StartsWith('-');
        var name = descending ? value[1..] : value;

        return Enum.TryParse(name, ignoreCase: true, out field) && Enum.IsDefined(field);
    }
}
