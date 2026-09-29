using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Exceptions.Categories;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>
/// FR-CAT-005 – xóa danh mục (Admin). Còn công thức tham chiếu → <see cref="CategoryHasRecipesException"/> (409 CATEGORY_DELETE_HAS_RECIPES) kèm số lượng;
/// rỗng → SOFT DELETE (MT-05/MT-20.11 – toàn hệ thống dùng IsDeleted, tuyệt đối không xóa vật lý).
/// </summary>
public sealed record DeleteCategoryCommand(Guid Id) : IRequest, ICacheInvalidator
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [CategoryCacheKeys.All, CategoryCacheKeys.DetailPrefix];
}

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() =>
        RuleFor(x => x.Id).NotEmpty().WithMessage("Thiếu định danh danh mục.");
}

public sealed class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new CategoryNotFoundException(request.Id);

        var recipeCount = await unitOfWork.Categories.CountActiveRecipesAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (recipeCount > 0)
        {
            // Số lượng nằm cả trong detail lẫn extension "recipeCount": Admin cần biết quy mô việc phải chuyển
            // trước khi quyết định, và Frontend đọc số trực tiếp thay vì tách từ chuỗi tiếng Việt (FR-CAT-005).
            throw new CategoryHasRecipesException(recipeCount);
        }

        // Soft delete theo BaseEntity (§7.1): Global Query Filter sẽ tự ẩn khỏi mọi truy vấn sau đó.
        category.SoftDelete();
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
