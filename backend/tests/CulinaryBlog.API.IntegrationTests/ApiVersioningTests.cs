using System.Net;
using System.Text.Json;

namespace CulinaryBlog.API.IntegrationTests;

public sealed class ApiVersioningTests(CulinaryBlogApiFactory factory) : IClassFixture<CulinaryBlogApiFactory>
{
    [Fact]
    public async Task V1RouteWorksAndReportsSupportedVersion()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/v1/categories").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("1.0", string.Join(',', response.Headers.GetValues("api-supported-versions")), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GET", "/api/v2/categories")]
    [InlineData("GET", "/api/v2/auth/me")]
    [InlineData("POST", "/api/v2/auth/refresh")]
    public async Task UnsupportedV2UsesValidationProblem(string method, string path)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await client.SendAsync(request).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
        Assert.Equal("VALIDATION_ERROR", json.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task UnversionedRouteDoesNotBecomeAnAliasAndQueryCannotSelectV2()
    {
        using var client = factory.CreateClient();
        using var missing = await client.GetAsync("/api/categories").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categories?api-version=2");
        request.Headers.Add("api-version", "2");
        using var response = await client.SendAsync(request).ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("1.0", string.Join(',', response.Headers.GetValues("api-supported-versions")), StringComparison.Ordinal);
    }
}
