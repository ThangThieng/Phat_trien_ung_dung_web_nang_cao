using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Exceptions.Recipes;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// CR-2026-04 (d) / MT-62 — <c>GET /recipes/mine/{id}</c>: chi tiết đầy đủ một công thức ở MỌI trạng thái cho chủ sở hữu hoặc
/// Admin, kèm <c>rowVersion</c> — nguồn dữ liệu của trang sửa <c>/dashboard/recipes/[id]/edit</c> và <c>rowVersion</c> ban đầu
/// cho <c>PUT /recipes/{id}</c> (FR-RCP-004). Riêng tư: không <see cref="ICacheable"/>, endpoint gắn <c>no-store</c> + <c>ETag</c>.
/// </summary>
public sealed record GetMyRecipeByIdQuery(Guid Id) : IRequest<RecipeDetailDto>;

public sealed class GetMyRecipeByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IRecipeReadRepository reader,
    IAuthorizationService authorization,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyRecipeByIdQuery, RecipeDetailDto>
{
    /// <summary>
    /// FR-RCP-011 A5: không tồn tại, đã xóa mềm, hoặc thuộc người khác (người gọi không phải Admin) → CÙNG một phản hồi 404
    /// <c>RECIPE_NOT_FOUND</c>, không phải 403 — endpoint không thể bị dùng để dò id nào đang tồn tại (tiền lệ FR-AUTH-009).
    /// Quyền chủ sở hữu/Admin kiểm bằng chính <see cref="RecipeAuthorizationHandler"/> của mọi command ghi — một chỗ duy nhất
    /// định nghĩa "ai được đụng vào công thức này". Chỉ cần <c>AuthorId</c> để phân quyền nên nạp aggregate không kèm con.
    /// </summary>
    public async Task<RecipeDetailDto> Handle(GetMyRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recipe = await unitOfWork.Recipes.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new RecipeNotFoundException(request.Id);

        var result = await authorization
            .AuthorizeAsync(currentUser.Principal, recipe, ResourceOwnerRequirement.Instance)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new RecipeNotFoundException(request.Id);
        }

        return await reader.GetDetailByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new RecipeNotFoundException(request.Id);
    }
}
