namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Lỗi tầng Application có Application Error Code (Phụ lục B) và TỰ mang mã HTTP. GlobalExceptionMiddleware chuyển
/// thành RFC 7807: "type" = <see cref="ErrorCode"/>, "status" = <see cref="StatusCode"/>.
/// Từ Buổi 3 chỉ dùng cho lỗi KHÔNG phải bất biến nghiệp vụ: phân quyền trên một tài nguyên cụ thể (RECIPE_FORBIDDEN,
/// FILE_FORBIDDEN), lỗi đầu vào tệp (FILE_*), dịch vụ ngoài/hạ tầng không trả lời (502, 503). Bất biến nghiệp vụ dùng
/// lớp con của CulinaryBlog.Domain.Exceptions.DomainException.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string errorCode, int statusCode, string title, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Title = title;
    }

    public string ErrorCode { get; }

    public int StatusCode { get; }

    public string Title { get; }

    /// <summary>Thông tin bổ sung đưa vào "extensions" của Problem Details (ví dụ: lockoutEnd).</summary>
    public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();
}

public sealed class BadRequestException(string errorCode, string message)
    : AppException(errorCode, 400, "Bad Request", message);

public sealed class UnauthorizedException(string errorCode, string message)
    : AppException(errorCode, 401, "Unauthorized", message);

public sealed class ForbiddenException(string errorCode, string message)
    : AppException(errorCode, 403, "Forbidden", message);

public sealed class ServiceUnavailableException(string errorCode, string message)
    : AppException(errorCode, 503, "Service Unavailable", message);

/// <summary>Dịch vụ bên ngoài trả lỗi hoặc không truy cập được (ví dụ Google JWKS — AUTH_GOOGLE_UNAVAILABLE, FR-AUTH-003 A3).</summary>
public sealed class BadGatewayException(string errorCode, string message)
    : AppException(errorCode, 502, "Bad Gateway", message);
