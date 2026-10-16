using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>
/// FR-CAT-004 – cập nhật danh mục (Admin). Đổi sang tên đã có ở danh mục KHÁC → 409 CATEGORY_NAME_EXISTS.
/// Slug KHÔNG đổi dù Name đổi (NFR-SEO-004 – không có bảng lịch sử slug nên đổi slug sẽ làm chết link cũ).
/// </summary>
public sealed record UpdateCategoryCommand(Guid Id, string Name, string? Description, string? ImageUrl, int OrderIndex = 0)
    : IRequest<CategoryDto>, ICacheInvalidator
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [CategoryCacheKeys.All];
}

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Thiếu định danh danh mục.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống.")
            .Length(CategoryLimits.NameMin, CategoryLimits.NameMax)
                .WithMessage($"Tên danh mục phải từ {CategoryLimits.NameMin} đến {CategoryLimits.NameMax} ký tự.")
            .Must(CategoryValidationRules.NotContainHtml).WithMessage("Tên danh mục không được chứa ký tự < hoặc >.");

        RuleFor(x => x.Description)
            .MaximumLength(CategoryLimits.DescriptionMax)
                .WithMessage($"Mô tả tối đa {CategoryLimits.DescriptionMax} ký tự.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(CategoryLimits.ImageUrlMax)
                .WithMessage($"Đường dẫn ảnh tối đa {CategoryLimits.ImageUrlMax} ký tự.")
            .Must(CategoryValidationRules.BeAbsoluteUrlOrEmpty).WithMessage("Đường dẫn ảnh không hợp lệ.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải >= 0.");
    }
}

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categories,
    ICategoryReadRepository readCategories,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categories.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(ErrorCodes.CategoryNotFound, "Không tìm thấy danh mục cần cập nhật.");

        // MT-36: FR-CAT-004 có cùng lỗ hổng với FR-CAT-003 – đổi sang tên đã tồn tại mà không kiểm tra
        // trước sẽ rơi xuống UNIQUE của DB và thành 500. excludeId để không tự báo trùng với chính nó.
        if (await categories.ExistsByNameAsync(request.Name, excludeId: request.Id, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException(
                ErrorCodes.CategoryNameExists,
                $"Danh mục '{request.Name.Trim()}' đã tồn tại.");
        }

        category.Update(request.Name, request.Description, request.ImageUrl, request.OrderIndex);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Đọc lại qua read repository để recipeCount tính đúng theo quy ước FR-CAT-001 (chỉ đếm Published),
        // thay vì tự đếm lại ở đây với một quy ước thứ hai.
        return await readCategories.GetBySlugAsync(category.Slug, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(ErrorCodes.CategoryNotFound, "Không tìm thấy danh mục cần cập nhật.");
    }
}
