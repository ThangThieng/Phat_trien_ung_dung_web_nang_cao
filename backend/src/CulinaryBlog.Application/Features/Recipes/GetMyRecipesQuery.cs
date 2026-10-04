using System.Globalization;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// FR-RCP-011 — danh sách công thức CỦA CHÍNH người gọi ở mọi trạng thái (Draft / Published / Archived). Endpoint RIÊNG TƯ
/// duy nhất của danh sách công thức, phần đối xứng của nguyên tắc cách ly Public/Private (MT-34): response phụ thuộc hoàn toàn
/// vào danh tính nên query này CỐ Ý KHÔNG implement <see cref="ICacheable"/> — <c>CachingBehavior</c> bỏ qua hoàn toàn, và
/// endpoint gắn <c>Cache-Control: no-store</c>. Admin truyền <c>authorId</c> để xem danh sách của một tác giả bất kỳ.
/// </summary>
public sealed record GetMyRecipesQuery(
    RecipeStatus? Status = null,
    int Page = 1,
    int PageSize = RecipeQueryRules.DefaultPageSize,
    string? SortBy = null,
    string? SortOrder = null,
    Guid? AuthorId = null) : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetMyRecipesQueryValidator : AbstractValidator<GetMyRecipesQuery>
{
    public GetMyRecipesQueryValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("status chỉ chấp nhận: Draft, Published, Archived.");
        RuleFor(x => x.Page).ValidPage();
        RuleFor(x => x.PageSize).ValidPageSize();
        RuleFor(x => x.SortBy).ValidSortBy();
        RuleFor(x => x.SortOrder).ValidSortOrder();
    }
}

public sealed class GetMyRecipesQueryHandler(IRecipeReadRepository recipes, ICurrentUser currentUser)
    : IRequestHandler<GetMyRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(GetMyRecipesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var callerId = currentUser.UserId
            ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Thiếu danh tính người gọi.");

        // Điều kiện tiên quyết 5 + bước 4: authorId chỉ được chấp nhận khi người gọi là Admin; ngược lại 403 RECIPE_FORBIDDEN (A2).
        if (request.AuthorId.HasValue && !currentUser.IsAdmin)
        {
            throw new ForbiddenException(ErrorCodes.RecipeForbidden, "Chỉ Admin được xem danh sách công thức của tác giả khác (authorId).");
        }

        // Bước 5: targetAuthorId = authorId (Admin truyền) hoặc chính người gọi. Id của ASP.NET Identity là GUID dạng chuỗi "D".
        var targetAuthorId = request.AuthorId?.ToString("D", CultureInfo.InvariantCulture) ?? callerId;

        var criteria = new RecipeListCriteria(
            request.Page,
            request.PageSize,
            RecipeFilterSpec.None,
            SortMapper.Map(request.SortBy, request.SortOrder));

        return recipes.GetByAuthorPagedAsync(targetAuthorId, request.Status, criteria, cancellationToken);
    }
}
