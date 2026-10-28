using System.Collections.Frozen;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace CulinaryBlog.API.Logging;

/// <summary>
/// NFR-SEC-007 (Buổi 8): danh sách tên thuộc tính KHÔNG BAO GIỜ được ghi giá trị vào log — mật khẩu, token (refresh/access/Google
/// <c>idToken</c>/token đặt lại mật khẩu của FR-AUTH-011) và header <c>Authorization</c>. So khớp không phân biệt hoa/thường để
/// bắt cả <c>{RefreshToken}</c> của message template lẫn <c>refreshToken</c> của DTO.
/// </summary>
public static class SensitiveData
{
    public const string Mask = "***";

    private static readonly FrozenSet<string> Names = new[]
    {
        "password",
        "currentPassword",
        "newPassword",
        "confirmPassword",
        "refreshToken",
        "accessToken",
        "idToken",
        "token",
        "authorization",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsSensitive(string propertyName) => Names.Contains(propertyName);
}

/// <summary>
/// Che thuộc tính nhạy cảm khi một đối tượng được ghi có cấu trúc (<c>{@Command}</c>): đối tượng có ít nhất một thuộc tính trong
/// <see cref="SensitiveData"/> được dựng lại với giá trị <see cref="SensitiveData.Mask"/> ở các thuộc tính đó; đối tượng khác để
/// Serilog xử lý như thường. Code hiện tại KHÔNG ghi payload nào (<c>LoggingBehavior</c> chỉ ghi tên command) — policy này là lớp
/// chặn cho câu log tương lai lỡ tay ghi cả DTO.
/// </summary>
public sealed class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(propertyValueFactory);

        var properties = value.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToArray();
        if (!properties.Any(p => SensitiveData.IsSensitive(p.Name)))
        {
            result = null!;
            return false;
        }

        var structured = new List<LogEventProperty>(properties.Length);
        foreach (var property in properties)
        {
            structured.Add(SensitiveData.IsSensitive(property.Name)
                ? new LogEventProperty(property.Name, new ScalarValue(SensitiveData.Mask))
                : new LogEventProperty(property.Name, propertyValueFactory.CreatePropertyValue(ReadSafely(property, value), destructureObjects: true)));
        }

        result = new StructureValue(structured, value.GetType().Name);
        return true;
    }

    private static object? ReadSafely(PropertyInfo property, object target)
    {
        try
        {
            return property.GetValue(target);
        }
        catch (TargetInvocationException ex)
        {
            return $"<{ex.InnerException?.GetType().Name ?? ex.GetType().Name}>";
        }
    }
}

/// <summary>
/// Lớp chặn cuối trước sink: thuộc tính cấp cao nhất của log event (tên trong message template, LogContext, scope) có tên nhạy cảm
/// thì bị thay bằng <see cref="SensitiveData.Mask"/>, kể cả khi giá trị là chuỗi thường chứ không phải đối tượng — trường hợp
/// <see cref="SensitiveDataDestructuringPolicy"/> không thấy. Message được render từ thuộc tính lúc ghi ra sink nên cũng bị che.
/// </summary>
public sealed class SensitivePropertyRedactor : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        foreach (var name in logEvent.Properties.Keys.Where(SensitiveData.IsSensitive).ToArray())
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(name, new ScalarValue(SensitiveData.Mask)));
        }
    }
}
