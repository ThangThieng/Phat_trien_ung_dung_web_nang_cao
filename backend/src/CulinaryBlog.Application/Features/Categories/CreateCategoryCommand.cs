using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions.Categories;
using CulinaryBlog.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>
/// FR-CAT-003 – tạo danh mục mới (Admin). Slug sinh tự động từ Name; trùng Name → <see cref="CategoryNameAlreadyExistsException"/>
/// (409 CATEGORY_NAME_EXISTS). Thành công thì xóa cache "categories:all" qua <see cref="ICacheInvalidator"/>.
/// </summary>
public sealed record CreateCategoryCommand(string Name, string? Description, string? ImageUrl, int OrderIndex = 0)
    : IRequest<CategoryDto>, ICacheInvalidator
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [CategoryCacheKeys.All, CategoryCacheKeys.DetailPrefix];
}

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống.")
            .Length(CategoryLimits.NameMin, CategoryLimits.NameMax)
                .WithMessage($"Tên danh mục phải từ {CategoryLimits.NameMin} đến {CategoryLimits.NameMax} ký tự.")
            .Must(CategoryValidationRules.NotContainHtml).WithMessage("Tên danh mục không được chứa ký tự < hoặc >.")
            .Must(CategoryValidationRules.ProducesUsableSlug)
                .WithMessage("Tên danh mục phải chứa ít nhất một chữ cái hoặc chữ số để sinh được đường dẫn.");

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

public sealed class CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // MT-36: kiểm tra trùng Name CHỦ ĐỘNG ở tầng Application trước khi ghi.
        // Nếu để ràng buộc UNIQUE của PostgreSQL bắt hộ thì người dùng nhận HTTP 500 thay vì lỗi nghiệp vụ.
        // CategoryPersistenceExceptionTranslator (Infrastructure) chỉ là lớp phòng vệ THỨ HAI cho tình huống đua.
        if (await unitOfWork.Categories.ExistsByNameAsync(request.Name, excludeId: null, cancellationToken).ConfigureAwait(false))
        {
            throw new CategoryNameAlreadyExistsException(request.Name.Trim());
        }

        var slug = await ResolveSlugAsync(unitOfWork.Categories, request.Name, cancellationToken).ConfigureAwait(false);
        var category = Category.Create(request.Name, slug, request.Description, request.ImageUrl, request.OrderIndex);

        await unitOfWork.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Danh mục vừa tạo chưa thể có công thức nào → recipeCount = 0, không cần thêm một truy vấn đếm.
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            RecipeCount: 0);
    }

    /// <summary>
    /// FR-CAT-003 bước 6–7 – slug phải unique VÀ không trùng danh sách slug dành riêng (NFR-SEO-004 / MT-53);
    /// trùng thì thêm hậu tố -2, -3… Kiểm tra unique tính cả danh mục đã soft delete (IDX_Category_Slug là unique thường).
    /// </summary>
    private static async Task<string> ResolveSlugAsync(
        ICategoryRepository categories,
        string name,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slug.FromText(name);
        var number = 1;
        var candidate = baseSlug;
        while (candidate.IsReserved
            || await categories.SlugExistsAsync(candidate.Value, cancellationToken).ConfigureAwait(false))
        {
            number++;
            candidate = baseSlug.WithSuffix(number);
        }

        return candidate.Value;
    }
}

/// <summary>Quy tắc kiểm tra dùng chung cho FR-CAT-003 và FR-CAT-004 – giữ một nguồn duy nhất cho cả hai validator.</summary>
public static class CategoryValidationRules
{
    /// <summary>Chặn thẻ HTML thô trong tên hiển thị (phòng vệ sớm; FE vẫn escape khi render).</summary>
    public static bool NotContainHtml(string? value) =>
        value is null || (!value.Contains('<', StringComparison.Ordinal) && !value.Contains('>', StringComparison.Ordinal));

    /// <summary>Tên chỉ gồm ký tự đặc biệt sẽ sinh ra slug rỗng → Category.Create ném lỗi. Chặn từ validator để trả 400 thay vì 500.</summary>
    public static bool ProducesUsableSlug(string? value) =>
        string.IsNullOrWhiteSpace(value) || SlugHelper.Generate(value).Length > 0;

    public static bool BeAbsoluteUrlOrEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) || Uri.TryCreate(value, UriKind.Absolute, out _);
}
