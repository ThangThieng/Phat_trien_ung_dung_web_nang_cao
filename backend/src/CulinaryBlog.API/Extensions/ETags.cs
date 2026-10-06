namespace CulinaryBlog.API.Extensions;

/// <summary>
/// ETag / If-Match cho Optimistic Concurrency của FR-RCP-004 (SRS: "client gửi RowVersion hiện tại trong If-Match").
/// Giá trị ETag là rowVersion (base64) đặt trong dấu nháy kép theo RFC 9110; chấp nhận cả dạng weak <c>W/"..."</c> khi đọc.
/// </summary>
public static class ETags
{
    public static string Format(string rowVersion) => $"\"{rowVersion}\"";

    /// <summary>Lấy rowVersion từ header If-Match; header trống hoặc là "*" thì trả null (dùng rowVersion trong body).</summary>
    public static string? Parse(string? ifMatch)
    {
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return null;
        }

        var value = ifMatch.Trim();
        if (value.StartsWith("W/", StringComparison.Ordinal))
        {
            value = value[2..];
        }

        value = value.Trim('"');
        return value.Length == 0 || value == "*" ? null : value;
    }
}
