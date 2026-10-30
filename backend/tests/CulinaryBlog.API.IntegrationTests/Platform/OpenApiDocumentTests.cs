using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>NFR-MAINT-003 — tài liệu OpenAPI phải bao phủ đúng surface thật và giữ hợp đồng ổn định.</summary>
public sealed partial class OpenApiDocumentTests(OpenApiTestFactory factory) : IClassFixture<OpenApiTestFactory>
{
    private static readonly Lock DocumentLock = new();
    private static Task<string>? documentJsonTask;

    [Fact]
    public async Task Document_CoversEveryApiAndHealthEndpoint_WithCompleteMetadata()
    {
        using var document = await GetDocumentAsync();
        var operations = GetOperations(document.RootElement).ToArray();
        var endpoints = DocumentedEndpoints().ToArray();

        Assert.Equal(48, operations.Length);
        Assert.Equal(endpoints.Length, operations.Length);
        Assert.Equal(operations.Length, operations.Select(item => item.Operation.GetProperty("operationId").GetString()).Distinct(StringComparer.Ordinal).Count());

        foreach (var item in operations)
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Operation.GetProperty("operationId").GetString()), $"{item.Method} {item.Path} thiếu operationId.");
            Assert.False(string.IsNullOrWhiteSpace(item.Operation.GetProperty("summary").GetString()), $"{item.Method} {item.Path} thiếu summary.");
            Assert.False(string.IsNullOrWhiteSpace(item.Operation.GetProperty("description").GetString()), $"{item.Method} {item.Path} thiếu description.");
            Assert.True(item.Operation.GetProperty("tags").GetArrayLength() > 0, $"{item.Method} {item.Path} thiếu tags.");
        }
    }

    [Fact]
    public async Task Security_ExactlyMatchesAuthorizationMetadata()
    {
        using var document = await GetDocumentAsync();
        var operations = GetOperations(document.RootElement).ToDictionary(item => Key(item.Method, item.Path), StringComparer.Ordinal);

        foreach (var endpoint in DocumentedEndpoints())
        {
            var operation = operations[Key(endpoint.Method, endpoint.Route)];
            var requiresAuthorization = endpoint.Metadata.OfType<IAuthorizeData>().Any() && !endpoint.Metadata.OfType<IAllowAnonymous>().Any();
            Assert.Equal(requiresAuthorization, operation.Operation.TryGetProperty("security", out var security) && security.GetArrayLength() > 0);
        }
    }

    [Fact]
    public async Task ErrorResponses_UseProblemJson()
    {
        using var document = await GetDocumentAsync();
        foreach (var item in GetOperations(document.RootElement))
        {
            foreach (var response in item.Operation.GetProperty("responses").EnumerateObject())
            {
                if (!int.TryParse(response.Name, out var status) || status < 400)
                {
                    continue;
                }

                Assert.True(
                    response.Value.GetProperty("content").TryGetProperty("application/problem+json", out _),
                    $"{item.Method} {item.Path} response {response.Name} không dùng application/problem+json.");
            }
        }
    }

    [Fact]
    public async Task Document_MatchesCommittedSnapshot()
    {
        using var document = await GetDocumentAsync();

        // Ảnh chụp luôn dùng LF (.gitattributes: eol=lf). WriteIndented xuống dòng theo hệ điều hành — CRLF trên Windows — nên
        // so nguyên văn thì test đỏ ngay sau khi clone trên máy Windows dù tài liệu không đổi (đo ở Buổi 7).
        var actual = JsonNode.Parse(document.RootElement.GetRawText())!
            .ToJsonString(new JsonSerializerOptions { WriteIndented = true })
            .ReplaceLineEndings("\n") + "\n";
        var snapshot = FindRepositoryRoot().Combine("docs", "api", "openapi.v1.json");

        if (string.Equals(Environment.GetEnvironmentVariable("OPENAPI_SNAPSHOT_UPDATE"), "1", StringComparison.Ordinal))
        {
            Directory.CreateDirectory(snapshot.DirectoryName!);
            await File.WriteAllTextAsync(snapshot.FullName, actual);
            return;
        }

        Assert.True(snapshot.Exists, $"Thiếu snapshot {snapshot.FullName}. Chạy test với OPENAPI_SNAPSHOT_UPDATE=1 để tạo.");
        Assert.Equal((await File.ReadAllTextAsync(snapshot.FullName)).ReplaceLineEndings("\n"), actual);
    }

    private async Task<JsonDocument> GetDocumentAsync()
    {
        Task<string> documentTask;
        lock (DocumentLock)
        {
            documentTask = documentJsonTask ??= RequestDocumentAsync();
        }

        return JsonDocument.Parse(await documentTask);
    }

    private async Task<string> RequestDocumentAsync()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private IEnumerable<(string Method, string Route, EndpointMetadataCollection Metadata)> DocumentedEndpoints() =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText is { } route && (Canonical(route).StartsWith("/api/v1", StringComparison.Ordinal) || route.StartsWith("/health", StringComparison.Ordinal)))
            .SelectMany(endpoint =>
            {
                var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [Microsoft.AspNetCore.Http.HttpMethods.Get];
                return methods.Select(method => (method.ToUpperInvariant(), NormalizeRoute(Canonical(endpoint.RoutePattern.RawText!)), endpoint.Metadata));
            });

    private static IEnumerable<(string Method, string Path, JsonElement Operation)> GetOperations(JsonElement root)
    {
        foreach (var path in root.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject().Where(property => SupportedMethods.Contains(property.Name)))
            {
                yield return (operation.Name.ToUpperInvariant(), NormalizeRoute(path.Name), operation.Value);
            }
        }
    }

    private static FileInfo FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "backend", "CulinaryBlog.sln")))
            {
                return new FileInfo(Path.Combine(directory.FullName, "repository.root"));
            }
        }

        throw new DirectoryNotFoundException("Không tìm thấy thư mục gốc repository.");
    }

    private static string Key(string method, string route) => $"{method.ToUpperInvariant()} {NormalizeRoute(route)}";

    /// <summary>Route thật "/api/v{version:apiVersion}/…" (Asp.Versioning) → "/api/v1/…" như trong tài liệu và ApiSurfaceTests.</summary>
    private static string Canonical(string route) => route.Replace("v{version:apiVersion}", "v1", StringComparison.Ordinal);

    private static string NormalizeRoute(string route) => RouteParameterRegex().Replace(route.TrimEnd('/'), "{$1}");

    private static readonly HashSet<string> SupportedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "get", "post", "put", "patch", "delete", "options", "head", "trace",
    };

    [GeneratedRegex("\\{(?:\\*\\*)?([^}:]+)(?::[^}]+)?\\}", RegexOptions.CultureInvariant)]
    private static partial Regex RouteParameterRegex();
}

internal static class FileInfoExtensions
{
    public static FileInfo Combine(this FileInfo marker, params string[] parts)
    {
        var path = marker.DirectoryName!;
        foreach (var part in parts)
        {
            path = Path.Combine(path, part);
        }

        return new FileInfo(path);
    }
}
