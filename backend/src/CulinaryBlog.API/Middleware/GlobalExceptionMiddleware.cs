using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CulinaryBlog.API.Middleware;

/// <summary>
/// CONS-005 / NFR-USE-003: mọi lỗi trả về RFC 7807 (application/problem+json), "type" = Application Error Code.
/// Lỗi không xử lý → 500, log đầy đủ, KHÔNG lộ stack trace (NFR-REL-002).
/// </summary>
public sealed partial class GlobalExceptionMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
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

    private static ProblemDetails ToProblem(Exception exception, HttpContext context)
    {
        switch (exception)
        {
            case AppException app:
                var appProblem = new ProblemDetails
                {
                    Type = app.ErrorCode,
                    Title = app.Title,
                    Status = app.StatusCode,
                    Detail = app.Message,
                    Instance = context.Request.Path,
                };
                foreach (var (key, value) in app.Extensions)
                {
                    appProblem.Extensions[key] = value;
                }

                return appProblem;

            case ValidationException validation:
                // Quyết định dự án: lỗi validation → 422 (theo các FR chi tiết)
                var errors = validation.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                return new ValidationProblemDetails(errors)
                {
                    Type = ErrorCodes.ValidationError,
                    Title = "Dữ liệu không hợp lệ",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = "Một hoặc nhiều trường không hợp lệ. Xem \"errors\".",
                    Instance = context.Request.Path,
                };

            case DomainException domain:
                return new ProblemDetails
                {
                    Type = ErrorCodes.ValidationError,
                    Title = "Vi phạm quy tắc nghiệp vụ",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = domain.Message,
                    Instance = context.Request.Path,
                };

            case BadHttpRequestException badRequest:
                return new ProblemDetails
                {
                    Type = ErrorCodes.ValidationError,
                    Title = "Bad Request",
                    Status = badRequest.StatusCode,
                    Detail = "Request không hợp lệ (tham số hoặc body sai định dạng).",
                    Instance = context.Request.Path,
                };

            // FR-CAT-003/004 (MT-36) – lớp phòng vệ THỨ HAI: hai Admin tạo/đổi trùng tên gần như đồng thời
            // có thể cùng vượt qua bước kiểm tra chủ động ở Application, chỉ ràng buộc UNIQUE của PostgreSQL
            // chặn lại. Thiếu nhánh này thì người dùng nhận 500 thay vì 409 – đúng lỗi mà MT-36 mô tả.
            // Chỉ nhận diện index của bảng Categories để không đổi hành vi của entity thuộc dev khác.
            case DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } unique }
                when IsCategoryUniqueIndex(unique.ConstraintName):
                return new ProblemDetails
                {
                    Type = ErrorCodes.CategoryNameExists,
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Tên danh mục đã tồn tại. Vui lòng chọn tên khác.",
                    Instance = context.Request.Path,
                };

            default:
                return new ProblemDetails
                {
                    Type = ErrorCodes.InternalError,
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Instance = context.Request.Path,
                };
        }
    }

    /// <summary>Hai index UNIQUE của bảng Categories (§7.6). Slug sinh từ Name nên cả hai đều quy về CATEGORY_NAME_EXISTS – Phụ lục B không có mã riêng cho slug.</summary>
    private static bool IsCategoryUniqueIndex(string? constraintName) =>
        constraintName is "IDX_Category_Name" or "IDX_Category_Slug";

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, string method, PathString path, Exception exception);
}
