namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Lớp gốc của mọi lỗi nghiệp vụ trong Domain (NFR-MAINT-004: bất biến nghiệp vụ viết bằng if/throw thuần, chỉ .NET BCL).
/// Exception chỉ mang <see cref="Code"/> = Application Error Code (SRS Phụ lục B) — KHÔNG mang mã HTTP:
/// mã HTTP do tầng API quyết định (ExceptionStatusMap), vì Domain không được biết mình đang chạy sau một web API.
/// Mỗi module có lớp gốc trừu tượng riêng trong <c>Exceptions/{Module}</c> (Auth, Recipes, Categories).
/// </summary>
public abstract class DomainException : Exception
{
    private readonly Dictionary<string, object?> _extensions = [];

    protected DomainException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    /// <summary>Application Error Code (SRS Phụ lục B) — thành trường "type" của RFC 7807.</summary>
    public string Code { get; }

    /// <summary>Dữ liệu bổ sung cho "extensions" của Problem Details (ví dụ recipeCount, lockoutEnd).</summary>
    public IReadOnlyDictionary<string, object?> Extensions => _extensions;

    protected void AddExtension(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _extensions[key] = value;
    }
}
