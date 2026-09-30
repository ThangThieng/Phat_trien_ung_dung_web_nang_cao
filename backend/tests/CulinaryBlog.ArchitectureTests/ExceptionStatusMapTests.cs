using CulinaryBlog.API.Middleware.ExceptionMapping;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.ArchitectureTests;

/// <summary>Hành vi của bảng "domain exception → mã HTTP" mà GlobalExceptionMiddleware dựa vào (Buổi 3).</summary>
public class ExceptionStatusMapTests
{
    [Fact]
    public void Resolve_UsesTheExactTypeRegistration()
    {
        var map = new ExceptionStatusMap().Map<ConcurrencyConflictException>(StatusCodes.Status409Conflict);

        var status = map.Resolve(new ConcurrencyConflictException("Category", Guid.NewGuid(), new InvalidOperationException()));

        Assert.Equal(StatusCodes.Status409Conflict, status);
    }

    [Fact]
    public void Resolve_FallsBackToTheNearestMappedBaseClass()
    {
        var map = new ExceptionStatusMap().Map<ModuleRootException>(StatusCodes.Status404NotFound);

        Assert.Equal(StatusCodes.Status404NotFound, map.Resolve(new ModuleLeafException()));
    }

    [Fact]
    public void Resolve_UnmappedException_Returns400()
    {
        Assert.Equal(ExceptionStatusMap.DefaultStatusCode, new ExceptionStatusMap().Resolve(new ModuleLeafException()));
        Assert.Equal(StatusCodes.Status400BadRequest, ExceptionStatusMap.DefaultStatusCode);
    }

    [Fact]
    public void Map_SameTypeTwice_Throws()
    {
        var map = new ExceptionStatusMap().Map<ModuleLeafException>(StatusCodes.Status409Conflict);

        Assert.Throws<InvalidOperationException>(() => map.Map<ModuleLeafException>(StatusCodes.Status400BadRequest));
    }

    [Fact]
    public void Map_NonErrorStatus_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExceptionStatusMap().Map<ModuleLeafException>(StatusCodes.Status200OK));

    [Fact]
    public void CommonMappings_AreRegisteredFromTheApiAssembly()
    {
        var map = ExceptionStatusMap.FromAssembly(typeof(CommonExceptionMappings).Assembly);

        Assert.Equal(StatusCodes.Status400BadRequest, map.Registrations[typeof(BusinessRuleViolationException)]);
        Assert.Equal(StatusCodes.Status409Conflict, map.Registrations[typeof(ConcurrencyConflictException)]);
    }

    private abstract class ModuleRootException(string code, string message) : DomainException(code, message);

    private sealed class ModuleLeafException() : ModuleRootException(ErrorCodes.ValidationError, "leaf");
}
