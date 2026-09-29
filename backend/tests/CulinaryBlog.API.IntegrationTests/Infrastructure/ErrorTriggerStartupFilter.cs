using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// CHỈ TỒN TẠI TRONG TEST (Buổi 3 — ProblemDetailsContractTests): gắn một middleware vào CUỐI pipeline của ứng dụng,
/// chạy khi request không khớp endpoint nào và đường dẫn bắt đầu bằng <see cref="PathPrefix"/>, rồi ném đúng loại lỗi
/// được yêu cầu. Lỗi đi ngược lên qua GlobalExceptionMiddleware thật — nhờ vậy test kiểm được hợp đồng Problem Details
/// cho từng loại exception mà không phải thêm endpoint giả vào code sản phẩm (ApiSurfaceTests sẽ không thấy chúng).
/// </summary>
public sealed class ErrorTriggerStartupFilter : IStartupFilter
{
    public const string PathPrefix = "/__tests/throw/";

    public const string SecretDetail = "chi-tiet-noi-bo-khong-duoc-lo-ra-ngoai";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);
        app.Use(async (context, nextMiddleware) =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (!path.StartsWith(PathPrefix, StringComparison.Ordinal))
            {
                await nextMiddleware(context).ConfigureAwait(false);
                return;
            }

            throw CreateException(path[PathPrefix.Length..]);
        });
    };

    private static Exception CreateException(string kind) => kind switch
    {
        "business-rule" => new BusinessRuleViolationException(ErrorCodes.RecipePublishIncomplete, "Thiếu bước hoặc nguyên liệu."),
        "unmapped-domain" => new UnmappedTestDomainException(),
        "domain-with-extensions" => new DomainWithExtensionsTestException(),
        "validation" => new ValidationException([new ValidationFailure("Title", "Tiêu đề phải có 5–200 ký tự.")]),
        "bad-gateway" => new BadGatewayException(ErrorCodes.AuthGoogleUnavailable, "Không truy cập được Google."),
        "unavailable" => new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "MinIO không khả dụng."),
        "unhandled" => new InvalidOperationException(SecretDetail),
        _ => new ArgumentOutOfRangeException(nameof(kind), kind, "Loại lỗi test không tồn tại."),
    };

    /// <summary>Một domain exception KHÔNG được đăng ký trong ExceptionStatusMap → phải rơi về mặc định 400.</summary>
    public sealed class UnmappedTestDomainException()
        : DomainException("TEST_UNMAPPED_RULE", "Quy tắc nghiệp vụ chưa đăng ký mã HTTP.");

    /// <summary>Domain exception mang Extensions → phải xuất hiện trong "extensions" của Problem Details.</summary>
    public sealed class DomainWithExtensionsTestException : DomainException
    {
        public DomainWithExtensionsTestException()
            : base(ErrorCodes.CategoryDeleteHasRecipes, "Danh mục còn chứa 7 công thức.")
        {
            AddExtension("recipeCount", 7);
        }
    }
}
