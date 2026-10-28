namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// FR-OBS-002 / CONS-010 (Buổi 8): CorrelationId của luồng xử lý hiện tại — request HTTP (<c>CorrelationIdMiddleware</c>) hoặc job
/// Hangfire mà request đó sinh ra. Handler cần chuyển mã này đi tiếp (ví dụ tham số job của FR-AUTH-011) thì đọc ở đây thay vì
/// tự đọc header.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>Null khi code chạy ngoài một request/job (khởi động, seed).</summary>
    string? CorrelationId { get; }
}
