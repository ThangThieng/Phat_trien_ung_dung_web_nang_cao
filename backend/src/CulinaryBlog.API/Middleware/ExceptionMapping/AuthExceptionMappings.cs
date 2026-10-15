using CulinaryBlog.Domain.Exceptions.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

public static class AuthExceptionMappings
{
    public static ProblemDetails ToProblem(AuthDomainException exception, string? instance)
    {
        var status = exception switch
        {
            InvalidCredentialsException or InvalidTokenException => StatusCodes.Status401Unauthorized,
            AccountDisabledException => StatusCodes.Status403Forbidden,
            UserNotFoundException => StatusCodes.Status404NotFound,
            EmailAlreadyExistsException => StatusCodes.Status409Conflict,
            AccountLockedException => StatusCodes.Status423Locked,
            _ => StatusCodes.Status500InternalServerError,
        };
        var problem = new ProblemDetails
        {
            Type = exception.Code,
            Title = status switch
            {
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                409 => "Conflict",
                423 => "Locked",
                _ => "Internal Server Error",
            },
            Status = status,
            Detail = exception.Message,
            Instance = instance,
        };
        if (exception is AccountLockedException locked)
        {
            problem.Extensions["lockoutEnd"] = locked.LockoutEnd;
            problem.Extensions["retryAfterMinutes"] = locked.RetryAfterMinutes;
        }

        return problem;
    }
}
