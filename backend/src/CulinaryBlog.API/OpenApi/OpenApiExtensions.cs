using System.Reflection;
using System.Text.Json.Nodes;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CulinaryBlog.API.OpenApi;

/// <summary>Đăng ký tài liệu OpenAPI v1 và các quy tắc chung của Culinary Blog API.</summary>
public static class OpenApiExtensions
{
    /// <summary>Thêm OpenAPI 3.1 cùng metadata, Bearer auth và hợp đồng lỗi RFC 7807.</summary>
    public static IServiceCollection AddCulinaryOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOpenApi("v1", options =>
        {
            options.ShouldInclude = _ => true;
            options.AddDocumentTransformer<ApiInfoDocumentTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<AuthorizationOperationTransformer>();
            options.AddOperationTransformer<ProblemDetailsOperationTransformer>();
        });
        return services;
    }
}

/// <summary>Chuẩn hóa thông tin cấp tài liệu và server tương đối để chạy trực tiếp hoặc qua Nginx.</summary>
internal sealed class ApiInfoDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        document.Info = new OpenApiInfo
        {
            Title = "Culinary Blog API",
            Version = "v1",
            Description = "REST API theo SRS Culinary Blog v1.2.3, các mục 8.1 đến 8.8.",
        };
        document.Servers = [new OpenApiServer { Url = "/", Description = "Máy chủ hiện tại" }];
        return Task.CompletedTask;
    }
}

/// <summary>Đăng ký HTTP Bearer/JWT trong components.securitySchemes.</summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    internal const string SchemeName = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Nhập access token JWT. Scalar tự thêm tiền tố Bearer.",
        };
        return Task.CompletedTask;
    }
}

/// <summary>Đánh dấu operation cần đăng nhập và bổ sung response 401/403.</summary>
internal sealed class AuthorizationOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any() || !metadata.OfType<IAuthorizeData>().Any())
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        var scheme = new OpenApiSecuritySchemeReference(BearerSecuritySchemeTransformer.SchemeName, context.Document, null);
        operation.Security.Add(new OpenApiSecurityRequirement { [scheme] = [] });
        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Access token thiếu, hết hạn hoặc không hợp lệ." });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Người dùng không có quyền thực hiện thao tác." });
        return Task.CompletedTask;
    }
}

/// <summary>Chuẩn hóa mọi response lỗi thành application/problem+json và công bố danh sách mã lỗi.</summary>
internal sealed class ProblemDetailsOperationTransformer : IOpenApiOperationTransformer
{
    private const string ProblemDetailsSchemaName = "ProblemDetails";
    private const string ErrorCodeSchemaName = "ErrorCode";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        var document = context.Document ?? throw new InvalidOperationException("OpenAPI document is unavailable.");
        EnsureSchemas(document);
        if (operation.Responses is null)
        {
            return Task.CompletedTask;
        }

        foreach (var (statusCode, response) in operation.Responses)
        {
            if (!int.TryParse(statusCode, out var code) || code < 400 || response is not OpenApiResponse concreteResponse)
            {
                continue;
            }

            concreteResponse.Content ??= new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase);
            concreteResponse.Content.Clear();
            concreteResponse.Content["application/problem+json"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchemaReference(ProblemDetailsSchemaName, document, null),
            };
        }

        return Task.CompletedTask;
    }

    private static void EnsureSchemas(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
        if (!document.Components.Schemas.ContainsKey(ErrorCodeSchemaName))
        {
            var errorCodes = typeof(ErrorCodes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                .Select(field => (string?)field.GetRawConstantValue())
                .Where(value => value is not null)
                .Order(StringComparer.Ordinal)
                .Select(value => (JsonNode)JsonValue.Create(value)!)
                .ToList();
            document.Components.Schemas[ErrorCodeSchemaName] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Mã lỗi ứng dụng trong SRS Phụ lục B.",
                Enum = errorCodes,
            };
        }

        if (!document.Components.Schemas.ContainsKey(ProblemDetailsSchemaName))
        {
            document.Components.Schemas[ProblemDetailsSchemaName] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "RFC 7807 Problem Details, trường type chứa mã lỗi ứng dụng.",
                Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal)
                {
                    ["type"] = new OpenApiSchemaReference(ErrorCodeSchemaName, document, null),
                    ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
                    ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                    ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
                    ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
                },
            };
        }
    }
}
