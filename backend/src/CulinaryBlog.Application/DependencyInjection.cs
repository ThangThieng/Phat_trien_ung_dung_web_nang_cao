using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Recipes;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Thứ tự pipeline theo SRS §6.3: Logging → Validation → Caching → Handler → CacheInvalidation
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddScoped<AuthResponseFactory>();
        services.AddScoped<RecipeLifecycle>();
        services.AddScoped<RecipeContent>();
        services.AddSingleton(TimeProvider.System);

        // NFR-SEC-006: resource-based authorization ở tầng Application (IAuthorizationService do tầng API đăng ký).
        services.AddSingleton<IAuthorizationHandler, RecipeAuthorizationHandler>();

        return services;
    }
}
