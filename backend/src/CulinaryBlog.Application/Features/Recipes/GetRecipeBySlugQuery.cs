using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>FR-RCP-002 – chi tiết công thức theo slug.</summary>
public sealed record GetRecipeBySlugQuery(string Slug) : IRequest<RecipeDetailDto>;

public sealed class GetRecipeBySlugQueryValidator : AbstractValidator<GetRecipeBySlugQuery>
{
    public GetRecipeBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(220);
    }
}

public sealed class GetRecipeBySlugQueryHandler(IRecipeReadRepository recipes, ICurrentUser currentUser)
    : IRequestHandler<GetRecipeBySlugQuery, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(GetRecipeBySlugQuery request, CancellationToken cancellationToken)
    {
        var result = await recipes.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(ErrorCodes.RecipeNotFound, $"Không tìm thấy công thức '{request.Slug}'.");

        var (recipe, authorId) = result;

        // A2: Draft/Archived chỉ tác giả sở hữu hoặc Admin được xem
        if (recipe.Status != RecipeStatus.Published && !currentUser.IsAdmin && currentUser.UserId != authorId)
        {
            throw new ForbiddenException(ErrorCodes.RecipeForbidden, "Bạn không có quyền xem công thức chưa xuất bản này.");
        }

        return recipe;
    }
}
