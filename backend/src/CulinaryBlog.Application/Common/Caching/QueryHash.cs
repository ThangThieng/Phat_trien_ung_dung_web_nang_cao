using System.Security.Cryptography;
using System.Text;

namespace CulinaryBlog.Application.Common.Caching;

/// <summary>
/// <c>{queryHash}</c> của bảng TTL chuẩn NFR-PERF-003: SHA-256 (hex chữ thường) của chuỗi tham số đã chuẩn hóa. Băm để khóa
/// Redis có độ dài cố định bất kể query dài bao nhiêu, và để giá trị người dùng nhập không nằm nguyên văn trong tên khóa.
/// </summary>
public static class QueryHash
{
    public static string Compute(string normalizedQuery)
    {
        ArgumentNullException.ThrowIfNull(normalizedQuery);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedQuery)));
    }
}
