using CulinaryBlog.API.Middleware.ExceptionMapping;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CulinaryBlog.API.Middleware;

/// <summary>
/// CONS-005 / NFR-USE-003: mọi lỗi trả về RFC 7807 (application/problem+json), "type" = Application Error Code.
/// Lỗi không xử lý → 500, log đầy đủ, KHÔNG lộ stack trace (NFR-REL-002).
///
/// Thứ tự xử lý (Buổi 3):
/// 1. <see cref="DomainException"/> — mã HTTP tra ở <see cref="ExceptionStatusMap"/> (Domain không mang mã HTTP),
///    "type" = Code của exception, Extensions của exception chép sang Problem Details.
/// 2. <see cref="ValidationException"/> — 400 VALIDATION_ERROR kèm "errors" theo field (MT-08, D-11).
/// 3. <see cref="AppException"/> — lỗi tầng Application tự mang mã HTTP (phân quyền tài nguyên, tệp, dịch vụ ngoài).
/// 4. <see cref="BadHttpRequestException"/> — body/tham số sai định dạng.
/// 5. Còn lại (kể cả DbUpdateException mà UnitOfWork không dịch được) — 500 INTERNAL_ERROR.
/// "traceId" được IProblemDetailsService tự thêm vào mọi response để tra log (Buổi 6 bổ sung CorrelationId).
/// </summary>
public sealed partial class GlobalExceptionMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ExceptionStatusMap exceptionStatusMap,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex) when (!context.Response.HasStarted && ex is not OperationCanceledException)
        {
            var problem = ToProblem(ex, context);
            context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

            if (problem.Status >= StatusCodes.Status500InternalServerError)
            {
                LogUnhandled(logger, context.Request.Method, context.Request.Path, ex);
            }

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem,
                Exception = ex,
            }).ConfigureAwait(false);
        }
    }

    private ProblemDetails ToProblem(Exception exception, HttpContext context)
    {
        switch (exception)
        {
            case DomainException domain:
                var status = exceptionStatusMap.Resolve(domain);
                return WithExtensions(
                    new ProblemDetails
                    {
                        Type = domain.Code,
                        Title = ReasonPhrases.GetReasonPhrase(status),
                        Status = status,
                        Detail = domain.Message,
                        Instance = context.Request.Path,
                    },
                    domain.Extensions);

            case ValidationException validation:
                // MT-08 (SRS v1.2.2 §3): MỌI lỗi validation/input → 400. Mã 422 bị loại bỏ khỏi hệ thống.
                var errors = validation.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                return new ValidationProblemDetails(errors)
                {
                    Type = ErrorCodes.ValidationError,
                    Title = "Dữ liệu không hợp lệ",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Một hoặc nhiều trường không hợp lệ. Xem \"errors\".",
                    Instance = context.Request.Path,
                };

            case AppException app:
                return WithExtensions(
                    new ProblemDetails
                    {
                        Type = app.ErrorCode,
                        Title = app.Title,
                        Status = app.StatusCode,
                        Detail = app.Message,
                        Instance = context.Request.Path,
                    },
                    app.Extensions.AsReadOnly());

            case BadHttpRequestException badRequest:
                return new ProblemDetails
                {
                    Type = ErrorCodes.ValidationError,
                    Title = "Bad Request",
                    Status = badRequest.StatusCode,
                    Detail = "Request không hợp lệ (tham số hoặc body sai định dạng).",
                    Instance = context.Request.Path,
                };

            default:
                return new ProblemDetails
                {
                    Type = ErrorCodes.InternalError,
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau và gửi kèm mã traceId nếu lỗi lặp lại.",
                    Instance = context.Request.Path,
                };
        }
    }

    private static ProblemDetails WithExtensions(ProblemDetails problem, IReadOnlyDictionary<string, object?> extensions)
    {
        foreach (var (key, value) in extensions)
        {
            problem.Extensions[key] = value;
        }

        return problem;
    }

    /// <summary>
    /// Khóa của "errors" theo camelCase của JSON body, TỪNG đoạn: "Title" → "title", "Steps[0].Title" → "steps[0].title"
    /// — để Frontend gắn lỗi vào đúng ô kể cả với mảng lồng nhau (D-11).
    /// </summary>
    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name)
            ? name
            : string.Join('.', name.Split('.').Select(part => part.Length == 0 ? part : char.ToLowerInvariant(part[0]) + part[1..]));

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, string method, PathString path, Exception exception);
}
