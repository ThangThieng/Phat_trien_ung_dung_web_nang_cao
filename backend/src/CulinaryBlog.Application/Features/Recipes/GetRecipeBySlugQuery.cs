using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions.Recipes;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-RCP-002 — chi tiết công thức CÔNG KHAI theo slug: chỉ phục vụ <c>Status == Published</c>, không đọc danh tính (MT-34,
/// retrofit D-4). Redis cache-aside <c>recipe:{slug}</c>, TTL 5 phút (retrofit D-6); các command nội dung/vòng đời xóa đúng
/// khóa này khi công thức thay đổi.
/// </summary>
public sealed record GetRecipeBySlugQuery(string Slug) : IRequest<RecipeDetailDto>, ICacheable
{
    public string CacheKey => RecipeCacheKeys.Detail(Slug);

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}

public sealed class GetRecipeBySlugQueryValidator : AbstractValidator<GetRecipeBySlugQuery>
{
    public GetRecipeBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(220);
    }
}

public sealed class GetRecipeBySlugQueryHandler(IRecipeReadRepository recipes)
    : IRequestHandler<GetRecipeBySlugQuery, RecipeDetailDto>
{
    /// <summary>
    /// A1 + A2: slug không tồn tại và công thức Draft/Archived trả CÙNG một lỗi 404 <c>RECIPE_NOT_FOUND</c> — không tiết lộ sự
    /// tồn tại của bản nháp (Buổi 2 trả 403 <c>RECIPE_FORBIDDEN</c>). Chủ sở hữu xem bản nháp qua <c>GET /recipes/mine/{id}</c>.
    /// </summary>
    public async Task<RecipeDetailDto> Handle(GetRecipeBySlugQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await recipes.GetPublishedBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false)
            ?? throw new RecipeNotFoundException(request.Slug);
    }
}
