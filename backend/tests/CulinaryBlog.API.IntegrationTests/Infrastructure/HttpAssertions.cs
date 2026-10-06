using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// Helper đọc RFC 7807 (CONS-005): mọi lỗi của hệ thống đều là <c>application/problem+json</c> với
/// <c>type</c> = Application Error Code (SRS Phụ lục B), nên test khẳng định mã lỗi chứ không khẳng định
/// câu chữ tiếng Việt — đổi thông điệp không được làm gãy test.
/// </summary>
public static class HttpAssertions
{
    /// <summary>Khớp cấu hình của API: enum đi trên dây dưới dạng chuỗi ("Hard"), không phải số (FR-SRCH-002).</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(body, Json)
            ?? throw new InvalidOperationException($"Không đọc được {typeof(T).Name} từ body: {body}");
    }

    /// <summary>Khẳng định response là Problem Details đúng status và đúng Application Error Code.</summary>
    public static async Task<ProblemDetails> ShouldBeProblemAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedErrorCode)
    {
        ArgumentNullException.ThrowIfNull(response);

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        Assert.True(
            expectedStatus == response.StatusCode,
            $"Mong đợi {(int)expectedStatus} nhưng nhận {(int)response.StatusCode}. Body: {body}");

        var problem = JsonSerializer.Deserialize<ProblemDetails>(body, Json)
            ?? throw new InvalidOperationException($"Body không phải Problem Details: {body}");

        Assert.Equal(expectedErrorCode, problem.Type);
        Assert.Equal((int)expectedStatus, problem.Status);

        return problem;
    }

    /// <summary>Lấy dictionary "errors" của ValidationProblemDetails (400 VALIDATION_ERROR – D-11/MT-08).</summary>
    public static async Task<IDictionary<string, string[]>> ReadValidationErrorsAsync(this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var problem = JsonSerializer.Deserialize<ValidationProblemDetails>(body, Json)
            ?? throw new InvalidOperationException($"Body không phải ValidationProblemDetails: {body}");

        return problem.Errors;
    }
}
