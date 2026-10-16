using Asp.Versioning;
using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.API.Extensions;

public static class ApiVersioningExtensions
{
    public const string RouteTemplate = "/api/v{version:apiVersion}";

    public static IServiceCollection AddUrlSegmentVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        return services;
    }

    public static RouteGroupBuilder MapV1Api(this WebApplication app)
    {
        var versions = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).ReportApiVersions().Build();
        return app.MapGroup(RouteTemplate).WithApiVersionSet(versions).MapToApiVersion(new ApiVersion(1, 0));
    }

    public static IServiceCollection AddVersionedOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options => options.AddDocumentTransformer((document, context, ct) =>
        {
            foreach (var path in document.Paths.ToArray())
            {
                var canonical = path.Key.Replace("{version:apiVersion}", "1", StringComparison.Ordinal)
                    .Replace("{version}", "1", StringComparison.Ordinal);
                if (canonical != path.Key)
                {
                    document.Paths.Remove(path.Key);
                    document.Paths.Add(canonical, path.Value);
                    if (path.Value.Operations is not null)
                    {
                        foreach (var operation in path.Value.Operations.Values.Where(operation => operation.Parameters is not null))
                        {
                            operation.Parameters = operation.Parameters!.Where(parameter => parameter.Name != "version").ToList();
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }));
        return services;
    }

    public static IApplicationBuilder UseUnsupportedVersionContract(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) && segments[1].StartsWith('v'))
        {
            var parser = context.RequestServices.GetRequiredService<IApiVersionParser>();
            if (parser.TryParse(segments[1].AsSpan(1), out var version) && version != new ApiVersion(1, 0))
            {
                // The library uses 404 for unsupported URL segments; Issue #46 explicitly requires 400.
                context.Response.Headers["api-supported-versions"] = "1.0";
                await Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    type: CulinaryBlog.Domain.Exceptions.ErrorCodes.ValidationError,
                    title: "Unsupported API version",
                    detail: "Phiên bản API không được hỗ trợ.")
                    .ExecuteAsync(context).ConfigureAwait(false);
                return;
            }
        }

        await next(context).ConfigureAwait(false);
    });
}
