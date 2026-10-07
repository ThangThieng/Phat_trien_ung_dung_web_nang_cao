using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Exceptions.Recipes;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// Bản đọc riêng tư phục vụ màn hình chỉnh sửa. Không implement ICacheable vì kết quả phụ thuộc danh tính người gọi.
/// Chủ sở hữu và Admin đọc được mọi trạng thái; người khác nhận 404 để không dò được id công thức.
/// </summary>
public sealed record GetMyRecipeByIdQuery(Guid RecipeId) : IRequest<RecipeDetailDto>;

public sealed class GetMyRecipeByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IRecipeReadRepository reader,
    IAuthorizationService authorization,
    ICurrentUser currentUser) : IRequestHandler<GetMyRecipeByIdQuery, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(GetMyRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Không dùng LoadForWriteAsync vì route đọc riêng tư phải trả 404 cho cả không tồn tại và không thuộc sở hữu.
        var recipe = await unitOfWork.Recipes.GetByIdWithDetailsAsync(request.RecipeId, cancellationToken).ConfigureAwait(false)
            ?? throw new RecipeNotFoundException(request.RecipeId);
        var result = await authorization.AuthorizeAsync(currentUser.Principal, recipe, ResourceOwnerRequirement.Instance)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new RecipeNotFoundException(request.RecipeId);
        }

        return await reader.GetDetailByIdAsync(recipe.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new RecipeNotFoundException(request.RecipeId);
    }
}
