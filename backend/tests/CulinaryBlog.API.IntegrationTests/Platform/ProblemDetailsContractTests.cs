using System.Net;
using System.Text.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// Buổi 3 — Dev 4: hợp đồng lỗi toàn cục (CONS-005, NFR-USE-003, NFR-REL-002). Mọi lỗi, dù sinh ra từ đâu, đều là
/// <c>application/problem+json</c> với <c>type</c> = Application Error Code và có <c>traceId</c> để tra log.
/// Các loại exception được ném qua <see cref="ErrorTriggerStartupFilter"/> (chỉ có trong test) để đi qua
/// GlobalExceptionMiddleware THẬT; 401 và 404 lấy từ pipeline thật của ứng dụng.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class ProblemDetailsContractTests(CulinaryBlogApiFactory factory)
{
    private const string ProblemJson = "application/problem+json";

    [Theory]
    [InlineData("business-rule", HttpStatusCode.BadRequest, ErrorCodes.RecipePublishIncomplete)]
    [InlineData("unmapped-domain", HttpStatusCode.BadRequest, "TEST_UNMAPPED_RULE")]
    [InlineData("validation", HttpStatusCode.BadRequest, ErrorCodes.ValidationError)]
    [InlineData("bad-gateway", HttpStatusCode.BadGateway, ErrorCodes.AuthGoogleUnavailable)]
    [InlineData("unavailable", HttpStatusCode.ServiceUnavailable, ErrorCodes.FileStorageUnavailable)]
    [InlineData("unhandled", HttpStatusCode.InternalServerError, ErrorCodes.InternalError)]
    public async Task EveryExceptionKind_BecomesProblemDetailsWithErrorCode(string kind, HttpStatusCode status, string errorCode)
    {
        var response = await factory.CreateClient().GetAsync(new Uri(ErrorTriggerStartupFilter.PathPrefix + kind, UriKind.Relative));

        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        await response.ShouldBeProblemAsync(status, errorCode);
        Assert.False(string.IsNullOrEmpty(await ReadStringAsync(response, "traceId")), "Problem Details phải có traceId để tra log.");
    }

    [Fact]
    public async Task ValidationError_ListsErrorsPerCamelCaseField()
    {
        var response = await factory.CreateClient().GetAsync(new Uri(ErrorTriggerStartupFilter.PathPrefix + "validation", UriKind.Relative));

        var errors = await response.ReadValidationErrorsAsync();
        Assert.Contains("title", errors.Keys);
    }

    [Fact]
    public async Task UnhandledError_DoesNotLeakInternalDetails()
    {
        var response = await factory.CreateClient().GetAsync(new Uri(ErrorTriggerStartupFilter.PathPrefix + "unhandled", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(ErrorTriggerStartupFilter.SecretDetail, body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
        Assert.DoesNotContain(" at ", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DomainExceptionExtensions_AreCopiedToProblemDetails()
    {
        var response = await factory.CreateClient().GetAsync(new Uri(ErrorTriggerStartupFilter.PathPrefix + "domain-with-extensions", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.CategoryDeleteHasRecipes);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(7, json.RootElement.GetProperty("recipeCount").GetInt32());
    }

    [Fact]
    public async Task MissingToken_Returns401ProblemDetails()
    {
        using var content = new MultipartFormDataContent();
        var response = await factory.CreateClient().PostAsync(new Uri("/api/v1/files/upload", UriKind.Relative), content);

        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, ErrorCodes.AuthTokenInvalid);
    }

    [Fact]
    public async Task UnknownRoute_Returns404ProblemDetails()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/v1/khong-ton-tai", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<string?> ReadStringAsync(HttpResponseMessage response, string property)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.TryGetProperty(property, out var value) ? value.GetString() : null;
    }
}
