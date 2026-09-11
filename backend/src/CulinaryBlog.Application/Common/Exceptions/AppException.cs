namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Lỗi nghiệp vụ có Application Error Code (Phụ lục B). GlobalExceptionMiddleware chuyển thành RFC 7807:
/// "type" = <see cref="ErrorCode"/>, "status" = <see cref="StatusCode"/>.
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

public sealed class NotFoundException(string errorCode, string message)
    : AppException(errorCode, 404, "Not Found", message);

public sealed class ConflictException(string errorCode, string message)
    : AppException(errorCode, 409, "Conflict", message);

public sealed class LockedException(string errorCode, string message)
    : AppException(errorCode, 423, "Locked", message);

public sealed class ServiceUnavailableException(string errorCode, string message)
    : AppException(errorCode, 503, "Service Unavailable", message);
