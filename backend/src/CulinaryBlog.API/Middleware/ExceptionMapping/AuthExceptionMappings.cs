using CulinaryBlog.Domain.Exceptions.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

public static class AuthExceptionMappings
{
    public static ProblemDetails ToProblem(AuthDomainException exception, string? instance)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var status = exception switch
        {
            InvalidTokenException => StatusCodes.Status401Unauthorized,
            AccountDisabledException => StatusCodes.Status403Forbidden,
            UserNotFoundException => StatusCodes.Status404NotFound,
            _ => throw new InvalidOperationException($"Missing Auth exception mapping: {exception.GetType().Name}"),
        };
        return new ProblemDetails
        {
            Type = exception.Code,
            Title = "Lỗi xác thực hoặc tài khoản",
            Status = status,
            Detail = exception.Message,
            Instance = instance,
        };
    }
}
