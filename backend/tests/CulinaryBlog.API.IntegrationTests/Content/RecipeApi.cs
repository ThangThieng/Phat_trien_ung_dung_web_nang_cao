using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Features.Recipes;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>Thao tác dựng dữ liệu qua chính API (không chèn thẳng DB) cho các lớp test module Recipe.</summary>
internal static class RecipeApi
{
    public static async Task<RecipeDetailDto> CreateDraftAsync(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title,
            description = $"Mô tả đầy đủ cho món {title} dùng trong kiểm thử tích hợp.",
            categoryId = TestDataSeeder.MonChinhCategoryId,
            prepTime = 20,
            cookTime = 40,
            servings = 4,
            difficulty = "Medium",
        });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"Tạo công thức thất bại ({(int)response.StatusCode}): {body}");
        return await response.ReadAsAsync<RecipeDetailDto>();
    }

    public static async Task<RecipeImageResultDto> UploadImageAsync(HttpClient client, Guid recipeId, byte[] content, string contentType, string fileName)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);

        var response = await client.PostAsync(new Uri($"/api/v1/recipes/{recipeId}/images", UriKind.Relative), form);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"Tải ảnh thất bại ({(int)response.StatusCode}): {body}");
        return await response.ReadAsAsync<RecipeImageResultDto>();
    }
}
