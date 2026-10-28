using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Infrastructure.Logging;

/// <summary>
/// FR-OBS-002 / CONS-010 (Buổi 8): CorrelationId của luồng xử lý hiện tại, lưu trong <see cref="AsyncLocal{T}"/> để đi theo mọi
/// <c>await</c> của request (hoặc job). Đặt bởi <c>CorrelationIdMiddleware</c> (API) và <see cref="Jobs.CorrelationIdJobFilter"/>
/// (worker Hangfire); filter phía client đọc ở đây lúc enqueue — filter Hangfire là đối tượng toàn cục, không có DI.
/// </summary>
public static partial class CorrelationContext
{
    /// <summary>Tên header (SRS §5.2) — Nginx sinh từ <c>$request_id</c> khi client không gửi, Frontend gửi <c>crypto.randomUUID()</c>.</summary>
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>Tên thuộc tính trong log event và trong tham số job Hangfire.</summary>
    public const string PropertyName = "CorrelationId";

    public const int MaxLength = 64;

    private static readonly AsyncLocal<string?> CurrentValue = new();

    public static string? Current => CurrentValue.Value;

    /// <summary>
    /// Giá trị nhận từ bên ngoài (header do client gửi, tham số job) chỉ được dùng khi gồm 1–64 ký tự <c>[A-Za-z0-9-]</c>. Header là
    /// dữ liệu client kiểm soát: chuỗi 10 KB làm ngập log, ký tự xuống dòng cho phép chèn dòng log giả (log injection).
    /// </summary>
    public static bool IsValid([NotNullWhen(true)] string? value) => value is { Length: > 0 and <= MaxLength } && SafeFormat().IsMatch(value);

    /// <summary>Mã mới khi không nhận được mã hợp lệ — cùng dạng UUID với <c>crypto.randomUUID()</c> của Frontend.</summary>
    public static string NewId() => Guid.NewGuid().ToString();

    /// <summary>Đặt CorrelationId cho phần còn lại của luồng; dispose để trả lại giá trị trước đó.</summary>
    public static IDisposable Begin(string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId);
        var previous = CurrentValue.Value;
        CurrentValue.Value = correlationId;
        return new Restore(previous);
    }

    [GeneratedRegex("^[A-Za-z0-9-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeFormat();

    private sealed class Restore(string? previous) : IDisposable
    {
        public void Dispose() => CurrentValue.Value = previous;
    }
}

/// <summary><see cref="ICorrelationIdAccessor"/> cho tầng Application (FR-AUTH-011 chuyển CorrelationId vào tham số job).</summary>
public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string? CorrelationId => CorrelationContext.Current;
}
