using System.Reflection;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

/// <summary>
/// Một module khai báo mã HTTP cho các domain exception của mình. Mỗi module MỘT lớp
/// (CommonExceptionMappings, AuthExceptionMappings, RecipeExceptionMappings, CategoryExceptionMappings);
/// mọi cài đặt trong assembly API được quét tự động — không ai phải sửa middleware hay Program.cs.
/// </summary>
public interface IExceptionStatusMapping
{
    void Configure(ExceptionStatusMap map);
}

/// <summary>
/// Bảng tra "kiểu domain exception → mã HTTP" (SRS v1.2.2 §6.2, MT-63). Domain exception chỉ mang Code nghiệp vụ;
/// mã HTTP là quyết định của tầng API và nằm ở đây.
/// Tra theo KIỂU (không theo chuỗi Code): trình biên dịch kiểm tra tên lớp, và một mã nghiệp vụ được phép đi với
/// hai mã HTTP khác nhau khi hai lớp khác nhau (AUTH_GOOGLE_TOKEN_INVALID: 400 và 401 — MT-61).
/// Tra đi ngược lên lớp cha, nên một lớp gốc của module có thể đặt mã mặc định cho mọi lớp con.
/// </summary>
public sealed class ExceptionStatusMap
{
    /// <summary>Mã dùng khi một domain exception chưa được đăng ký: vi phạm quy tắc nghiệp vụ mặc định là lỗi của request.</summary>
    public const int DefaultStatusCode = StatusCodes.Status400BadRequest;

    private readonly Dictionary<Type, int> _statusCodes = [];

    public IReadOnlyDictionary<Type, int> Registrations => _statusCodes;

    /// <summary>Dựng bảng từ mọi <see cref="IExceptionStatusMapping"/> không trừu tượng trong <paramref name="assembly"/>.</summary>
    public static ExceptionStatusMap FromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var map = new ExceptionStatusMap();
        var mappingTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IExceptionStatusMapping).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

        foreach (var type in mappingTypes)
        {
            ((IExceptionStatusMapping)Activator.CreateInstance(type)!).Configure(map);
        }

        return map;
    }

    public ExceptionStatusMap Map<TException>(int statusCode)
        where TException : DomainException
    {
        if (statusCode is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Domain exception phải ánh xạ tới mã lỗi 4xx/5xx.");
        }

        if (!_statusCodes.TryAdd(typeof(TException), statusCode))
        {
            throw new InvalidOperationException($"{typeof(TException).Name} đã được ánh xạ ở một module khác.");
        }

        return this;
    }

    /// <summary>Kiểu có được đăng ký TƯỜNG MINH không (không tính kế thừa) — dùng cho test kiến trúc.</summary>
    public bool IsExplicitlyMapped(Type exceptionType) => _statusCodes.ContainsKey(exceptionType);

    /// <summary>Mã HTTP của <paramref name="exception"/>: đăng ký của chính kiểu đó, nếu không có thì của lớp cha gần nhất.</summary>
    public int Resolve(DomainException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        for (var type = exception.GetType(); type is not null && type != typeof(DomainException); type = type.BaseType)
        {
            if (_statusCodes.TryGetValue(type, out var statusCode))
            {
                return statusCode;
            }
        }

        return DefaultStatusCode;
    }
}
