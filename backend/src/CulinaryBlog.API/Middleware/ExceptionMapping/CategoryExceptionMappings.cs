using CulinaryBlog.Domain.Exceptions.Categories;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

/// <summary>
/// Ánh xạ lỗi nghiệp vụ module Category sang mã HTTP (SRS §3.2, Phụ lục B). Domain không mang mã HTTP —
/// <c>ExceptionStatusMap.FromAssembly</c> quét và đăng ký lớp này tự động; test kiến trúc bắt lỗi nếu quên đăng ký.
/// </summary>
public sealed class CategoryExceptionMappings : IExceptionStatusMapping
{
    public void Configure(ExceptionStatusMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        map.Map<CategoryNotFoundException>(StatusCodes.Status404NotFound)
            .Map<CategoryNameAlreadyExistsException>(StatusCodes.Status409Conflict)
            .Map<CategoryHasRecipesException>(StatusCodes.Status409Conflict);
    }
}
