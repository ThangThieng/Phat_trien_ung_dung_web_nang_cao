using CulinaryBlog.Domain.Exceptions.Recipes;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

/// <summary>
/// Ánh xạ lỗi nghiệp vụ module Recipe sang mã HTTP (SRS §3.3, Phụ lục B). <c>RECIPE_FORBIDDEN</c> không nằm ở đây:
/// đó là quyết định phân quyền của <c>RecipeAuthorizationHandler</c> (ForbiddenException), không phải bất biến entity.
/// </summary>
public sealed class RecipeExceptionMappings : IExceptionStatusMapping
{
    public void Configure(ExceptionStatusMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        map.Map<RecipeNotFoundException>(StatusCodes.Status404NotFound)
            .Map<RecipeImageNotFoundException>(StatusCodes.Status404NotFound)
            .Map<RecipeConcurrencyException>(StatusCodes.Status409Conflict)
            .Map<InvalidRecipeStatusException>(StatusCodes.Status409Conflict)
            .Map<RecipeSlugConflictException>(StatusCodes.Status409Conflict)
            .Map<RecipePublishIncompleteException>(StatusCodes.Status400BadRequest);
    }
}
