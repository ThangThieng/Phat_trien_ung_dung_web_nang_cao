using CulinaryBlog.API.Middleware.ExceptionMapping;
using CulinaryBlog.Domain.Exceptions.Categories;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.ArchitectureTests;

/// <summary>SRS v1.2.2 Phụ lục B / FR-CAT-003–005: mã HTTP của ba lỗi nghiệp vụ module Category.</summary>
public class CategoryExceptionMappingsTests
{
    private readonly ExceptionStatusMap _map = ExceptionStatusMap.FromAssembly(typeof(CategoryExceptionMappings).Assembly);

    [Fact]
    public void CategoryNotFound_Maps404() =>
        Assert.Equal(StatusCodes.Status404NotFound, _map.Resolve(new CategoryNotFoundException(Guid.NewGuid())));

    [Fact]
    public void CategoryNameAlreadyExists_Maps409() =>
        Assert.Equal(StatusCodes.Status409Conflict, _map.Resolve(new CategoryNameAlreadyExistsException("Món chính")));

    [Fact]
    public void CategoryHasRecipes_Maps409AndCarriesRecipeCount()
    {
        var error = new CategoryHasRecipesException(7);

        Assert.Equal(StatusCodes.Status409Conflict, _map.Resolve(error));
        Assert.Equal(7, error.Extensions["recipeCount"]);
    }
}
