using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

/// <summary>
/// FR-CAT-005 – xóa danh mục (Admin). Còn công thức tham chiếu → 409 CATEGORY_DELETE_HAS_RECIPES kèm số lượng;
/// rỗng → SOFT DELETE (MT-05/MT-20.11 – toàn hệ thống dùng IsDeleted, tuyệt đối không xóa vật lý).
/// </summary>
public sealed record DeleteCategoryCommand(Guid Id) : IRequest, ICacheInvalidator
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [CategoryCacheKeys.All];
}

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() =>
        RuleFor(x => x.Id).NotEmpty().WithMessage("Thiếu định danh danh mục.");
}

public sealed class DeleteCategoryCommandHandler(ICategoryRepository categories, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categories.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(ErrorCodes.CategoryNotFound, "Không tìm thấy danh mục cần xóa.");

        var recipeCount = await categories.CountRecipesAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (recipeCount > 0)
        {
            // Trả kèm SỐ LƯỢNG trong detail: Admin cần biết quy mô việc phải chuyển trước khi quyết định,
            // và con số đó server đã đếm sẵn rồi (FR-CAT-005).
            throw new ConflictException(
                ErrorCodes.CategoryDeleteHasRecipes,
                $"Danh mục đang có {recipeCount} công thức. Hãy chuyển chúng sang danh mục khác trước khi xóa.");
        }

        // Soft delete theo BaseEntity (§7.1): Global Query Filter sẽ tự ẩn khỏi mọi truy vấn sau đó.
        category.IsDeleted = true;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
