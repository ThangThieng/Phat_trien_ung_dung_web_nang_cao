using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Content;

/// <summary>
/// Buổi 4 — Dev 2: FR-RCP-009 nguyên liệu, FR-RCP-010 bước nấu (server đánh số + reorder, D-16 trên PostgreSQL thật),
/// FR-RCP-004 cập nhật có Optimistic Concurrency, và hai mảng inline của POST /recipes.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class RecipeContentEndpointsTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>POST /recipes kèm 10 nguyên liệu + 5 bước → 201, StepNumber 1..5 do server gán.</summary>
    [Fact]
    public async Task CreateWithInlineStepsAndIngredients_Returns201WithServerNumberedSteps()
    {
        var author = factory.CreateClientAs("Author");

        var response = await author.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Bún bò Huế",
            description = "Bún bò Huế nước dùng đậm vị sả, mắm ruốc và sa tế cay nồng.",
            categoryId = TestDataSeeder.MonChinhCategoryId,
            prepTime = 45,
            cookTime = 180,
            servings = 6,
            difficulty = "Hard",
            ingredients = new object[]
            {
                new { name = "Xương ống heo", quantity = 1.5, unit = "kg" },
                new { name = "Bắp bò", quantity = 600, unit = "gram" },
                new { name = "Giò heo", quantity = 1, unit = "cái", notes = "chặt khoanh" },
                new { name = "Sả cây", quantity = 8, unit = "cây", notes = "đập dập" },
                new { name = "Mắm ruốc Huế", quantity = 2, unit = "thìa canh" },
                new { name = "Sa tế", quantity = 3, unit = "thìa canh" },
                new { name = "Bún sợi to", quantity = 1, unit = "kg" },
                new { name = "Hành tây", quantity = 1, unit = "củ" },
                new { name = "Muối, đường", quantityText = "vừa ăn" },
                new { name = "Rau sống", quantityText = "1 rổ", notes = "ăn kèm" },
            },
            steps = new object[]
            {
                new { title = "Sơ chế", description = "Rửa sạch xương, bắp bò, giò heo; chần qua nước sôi." },
                new { title = "Ninh nước dùng", description = "Ninh xương với sả và hành tây trong 2 giờ.", timerMinutes = 120 },
                new { title = "Luộc thịt", description = "Cho bắp bò và giò heo vào nồi, luộc tới khi chín mềm.", timerMinutes = 45 },
                new { title = "Nêm nếm", description = "Hòa mắm ruốc, nêm muối đường và sa tế cho vừa ăn." },
                new { title = "Hoàn thành", description = "Trụng bún, xếp thịt, chan nước dùng nóng." },
            },
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var recipe = await response.ReadAsAsync<RecipeDetailDto>();
        Assert.Equal(10, recipe.Ingredients.Count);
        Assert.Equal([1, 2, 3, 4, 5], recipe.Steps.Select(s => s.StepNumber));
        Assert.Equal("vừa ăn", recipe.Ingredients.Single(i => i.Name == "Muối, đường").QuantityText);
        Assert.Equal(new Uri($"/api/v1/recipes/mine/{recipe.Id}", UriKind.Relative), response.Headers.Location);
    }

    [Fact]
    public async Task CreateWithIngredientMissingAllQuantities_Returns400QuantityRequired()
    {
        var author = factory.CreateClientAs("Author");

        var response = await author.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Canh bí đao",
            description = "Canh bí đao nấu tôm khô thanh mát cho ngày hè.",
            categoryId = TestDataSeeder.MonChinhCategoryId,
            prepTime = 10,
            cookTime = 20,
            servings = 4,
            difficulty = "Easy",
            ingredients = new object[] { new { name = "Muối" } },
        });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.IngredientQuantityRequired);
    }

    [Fact]
    public async Task IngredientCrud_AddUpdateSwitchToTextDelete()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Chè bưởi");

        var added = await author.PostAsJsonAsync($"/api/v1/recipes/{draft.Id}/ingredients", new { name = "Đường cát", quantity = 150, unit = "gram" });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var ingredient = await added.ReadAsAsync<RecipeIngredientDto>();

        var updated = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}/ingredients/{ingredient.Id}", new { quantityText = "vừa ngọt" });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var afterUpdate = await updated.ReadAsAsync<RecipeIngredientDto>();
        Assert.Equal("Đường cát", afterUpdate.Name);
        Assert.Null(afterUpdate.Quantity);
        Assert.Equal("vừa ngọt", afterUpdate.QuantityText);

        var emptied = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}/ingredients/{ingredient.Id}", new { notes = "chỉ còn ghi chú" });
        await emptied.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.IngredientQuantityRequired);

        var deleted = await author.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}/ingredients/{ingredient.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var again = await author.DeleteAsync(new Uri($"/api/v1/recipes/{draft.Id}/ingredients/{ingredient.Id}", UriKind.Relative));
        await again.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);
    }

    [Fact]
    public async Task Ingredient_WithNegativeQuantity_Returns400Validation()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Gỏi cuốn tôm thịt");

        var response = await author.PostAsJsonAsync($"/api/v1/recipes/{draft.Id}/ingredients", new { name = "Tôm", quantity = -2, unit = "con" });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>D-16 trên PostgreSQL thật: hoán đổi bước 2 và 3 bằng reorder → 200, không vỡ UQ_RecipeStep_Recipe_StepNumber.</summary>
    [Fact]
    public async Task ReorderSwapSteps2And3_Returns200WithoutUniqueViolation()
    {
        var author = factory.CreateClientAs("Author");
        var (recipeId, steps) = await CreateDraftWithStepsAsync(author, "Cá kho làng Vũ Đại", 4);

        var response = await author.PatchAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/reorder",
            new { stepIds = new[] { steps[0].Id, steps[2].Id, steps[1].Id, steps[3].Id } });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        var reordered = await response.ReadAsAsync<List<RecipeStepDto>>();
        Assert.Equal([1, 2, 3, 4], reordered.Select(s => s.StepNumber));
        Assert.Equal([steps[0].Title, steps[2].Title, steps[1].Title, steps[3].Title], reordered.Select(s => s.Title));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var persisted = await db.RecipeSteps.AsNoTracking().Where(s => s.RecipeId == recipeId).OrderBy(s => s.StepNumber).Select(s => s.Title).ToListAsync();
        Assert.Equal(reordered.Select(s => s.Title), persisted);
    }

    [Fact]
    public async Task Reorder_WithMissingId_Returns400()
    {
        var author = factory.CreateClientAs("Author");
        var (recipeId, steps) = await CreateDraftWithStepsAsync(author, "Thịt kho tàu", 3);

        var response = await author.PatchAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/reorder", new { stepIds = new[] { steps[1].Id, steps[0].Id } });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    [Fact]
    public async Task DeleteMiddleStep_RenumbersRemaining_AndStepNumberNotAcceptedInBody()
    {
        var author = factory.CreateClientAs("Author");
        var (recipeId, steps) = await CreateDraftWithStepsAsync(author, "Canh chua cá bông lau", 3);

        var deleted = await author.DeleteAsync(new Uri($"/api/v1/recipes/{recipeId}/steps/{steps[1].Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        // stepNumber trong body bị bỏ qua (MT-03): server vẫn gán Max + 1 = 3.
        var added = await author.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new { title = "Dọn ra", description = "Múc canh ra tô.", stepNumber = 99 });
        Assert.Equal(3, (await added.ReadAsAsync<RecipeStepDto>()).StepNumber);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var numbers = await db.RecipeSteps.AsNoTracking().Where(s => s.RecipeId == recipeId).OrderBy(s => s.StepNumber).Select(s => s.StepNumber).ToListAsync();
        Assert.Equal([1, 2, 3], numbers);
    }

    [Fact]
    public async Task UpdateStep_ChangesContentOnly()
    {
        var author = factory.CreateClientAs("Author");
        var (recipeId, steps) = await CreateDraftWithStepsAsync(author, "Bánh canh cua", 2);

        var response = await author.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/{steps[1].Id}", new { description = "Nấu nước dùng cua trong 30 phút.", timerMinutes = 30 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var step = await response.ReadAsAsync<RecipeStepDto>();
        Assert.Equal(steps[1].Title, step.Title);
        Assert.Equal(2, step.StepNumber);
        Assert.Equal(30, step.TimerMinutes);
    }

    [Fact]
    public async Task ContentChange_ByAnotherAuthor_Returns403()
    {
        var owner = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(owner, "Bánh bèo chén");
        var other = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);

        var ingredient = await other.PostAsJsonAsync($"/api/v1/recipes/{draft.Id}/ingredients", new { name = "Bột gạo", quantity = 200, unit = "gram" });
        await ingredient.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);

        var update = await other.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { title = "Bánh bèo bị sửa", rowVersion = draft.RowVersion });
        await update.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);
    }

    // ---------- FR-RCP-004 ----------

    /// <summary>PUT với rowVersion cũ (người khác đã sửa trước) → 409 RECIPE_CONCURRENCY_CONFLICT.</summary>
    [Fact]
    public async Task Update_WithStaleRowVersion_Returns409Conflict()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bánh xèo tôm nhảy");

        var first = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { servings = 5, rowVersion = draft.RowVersion });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var stale = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { servings = 8, rowVersion = draft.RowVersion });

        await stale.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.RecipeConcurrencyConflict);
    }

    /// <summary>
    /// PUT chỉ đổi dinh dưỡng (Owned Entity) với rowVersion cũ vẫn phải 409: EF không tự đánh dấu Recipe là Modified khi
    /// chỉ owned entity đổi, nên nếu không ép thì câu UPDATE không so RowVersion và ghi đè âm thầm thay đổi của người khác.
    /// </summary>
    [Fact]
    public async Task Update_NutritionOnlyWithStaleRowVersion_Returns409Conflict()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Gỏi ngó sen tôm thịt");

        var first = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { nutrition = new { calories = 250 }, rowVersion = draft.RowVersion });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.NotEqual(draft.RowVersion, (await first.ReadAsAsync<RecipeDetailDto>()).RowVersion);

        var stale = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { nutrition = new { calories = 999 }, rowVersion = draft.RowVersion });

        await stale.ShouldBeProblemAsync(HttpStatusCode.Conflict, ErrorCodes.RecipeConcurrencyConflict);
    }

    /// <summary>If-Match (ETag) thay cho rowVersion trong body; response trả ETag mới dùng được cho lần sửa kế tiếp.</summary>
    [Fact]
    public async Task Update_WithIfMatchHeader_ReturnsNewETagUsableForNextUpdate()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Cơm chiên dương châu");

        using var first = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/recipes/{draft.Id}") { Content = JsonContent.Create(new { cookTime = 25 }) };
        first.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{draft.RowVersion}\""));
        var firstResponse = await author.SendAsync(first);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var etag = firstResponse.Headers.ETag;
        Assert.NotNull(etag);
        Assert.Equal($"\"{(await firstResponse.ReadAsAsync<RecipeDetailDto>()).RowVersion}\"", etag.Tag);

        using var second = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/recipes/{draft.Id}") { Content = JsonContent.Create(new { cookTime = 30 }) };
        second.Headers.IfMatch.Add(etag);
        var secondResponse = await author.SendAsync(second);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(30, (await secondResponse.ReadAsAsync<RecipeDetailDto>()).CookTimeMinutes);
    }

    [Fact]
    public async Task Update_WithoutRowVersion_Returns400()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Nem rán Hà Nội");

        var response = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { servings = 3 });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    /// <summary>MT-27: đổi tên bản nháp chưa từng xuất bản → slug đổi theo; đổi tên công thức đã từng xuất bản → slug giữ nguyên.</summary>
    [Fact]
    public async Task Update_Title_RegeneratesSlugOnlyForNeverPublishedDraft()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Chả giò rế");

        var renamedDraft = await (await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { title = "Chả giò rế tôm cua", rowVersion = draft.RowVersion }))
            .ReadAsAsync<RecipeDetailDto>();
        Assert.Equal("cha-gio-re-tom-cua", renamedDraft.Slug);

        // Công thức seed "Phở bò Hà Nội" đã Published → khóa slug; hủy xuất bản rồi đổi tên vẫn giữ slug.
        var published = await GetSeededDetailAsync(TestDataSeeder.PublishedRecipeSlug);
        var unpublished = await (await RecipeApi.PatchAsync(author, published.Id, "unpublish")).ReadAsAsync<RecipeDetailDto>();
        var renamed = await (await author.PutAsJsonAsync($"/api/v1/recipes/{published.Id}", new { title = "Phở bò tái nạm gầu", rowVersion = unpublished.RowVersion }))
            .ReadAsAsync<RecipeDetailDto>();
        Assert.Equal("Phở bò tái nạm gầu", renamed.Title);
        Assert.Equal(TestDataSeeder.PublishedRecipeSlug, renamed.Slug);
    }

    [Fact]
    public async Task Update_WithUnknownCategory_Returns400OnCategoryIdField()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Bò bía ngọt");

        var response = await author.PutAsJsonAsync($"/api/v1/recipes/{draft.Id}", new { categoryId = Guid.NewGuid(), rowVersion = draft.RowVersion });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("categoryId", (await response.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);
    }

    /// <summary>Luồng lỗi của ba endpoint bước nấu: body thiếu title → 400, bước không thuộc công thức → 404, người khác → 403.</summary>
    [Fact]
    public async Task StepEndpoints_ErrorPaths()
    {
        var author = factory.CreateClientAs("Author");
        var (recipeId, steps) = await CreateDraftWithStepsAsync(author, "Mì Quảng gà", 1);
        var other = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);

        var missingTitle = await author.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new { description = "Chỉ có mô tả." });
        await missingTitle.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("title", (await missingTitle.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);

        var updateUnknown = await author.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/{Guid.NewGuid()}", new { title = "Không có" });
        await updateUnknown.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);

        var deleteUnknown = await author.DeleteAsync(new Uri($"/api/v1/recipes/{recipeId}/steps/{Guid.NewGuid()}", UriKind.Relative));
        await deleteUnknown.ShouldBeProblemAsync(HttpStatusCode.NotFound, ErrorCodes.RecipeNotFound);

        var deleteByOther = await other.DeleteAsync(new Uri($"/api/v1/recipes/{recipeId}/steps/{steps[0].Id}", UriKind.Relative));
        await deleteByOther.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);

        var updateByOther = await other.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/{steps[0].Id}", new { title = "Sửa trộm" });
        await updateByOther.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.RecipeForbidden);
    }

    /// <summary>D-11: khóa của "errors" đúng tên trường JSON để FE gắn vào ô — body phẳng "name", mảng inline "steps[0].title".</summary>
    [Fact]
    public async Task ValidationErrorKeys_MatchJsonFieldNames()
    {
        var author = factory.CreateClientAs("Author");
        var draft = await RecipeApi.CreateDraftAsync(author, "Xôi gấc ngày Tết");

        var ingredient = await author.PostAsJsonAsync($"/api/v1/recipes/{draft.Id}/ingredients", new { quantity = 300, unit = "gram" });
        Assert.Contains("name", (await ingredient.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);

        var inline = await author.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = "Xôi gấc ngày Tết",
            description = "Xôi gấc đỏ tươi, dẻo thơm, dùng trong mâm cỗ ngày Tết.",
            categoryId = TestDataSeeder.MonChinhCategoryId,
            prepTime = 30,
            cookTime = 45,
            servings = 6,
            difficulty = "Medium",
            steps = new object[] { new { description = "Ngâm gạo nếp qua đêm." } },
        });
        await inline.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
        Assert.Contains("steps[0].title", (await inline.ReadValidationErrorsAsync()).Keys, StringComparer.Ordinal);
    }

    private static async Task<(Guid RecipeId, List<RecipeStepDto> Steps)> CreateDraftWithStepsAsync(HttpClient author, string title, int count)
    {
        var draft = await RecipeApi.CreateDraftAsync(author, title);
        var steps = new List<RecipeStepDto>();
        for (var i = 1; i <= count; i++)
        {
            var response = await author.PostAsJsonAsync($"/api/v1/recipes/{draft.Id}/steps", new { title = $"Bước {i} của {title}", description = $"Mô tả chi tiết bước {i}." });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            steps.Add(await response.ReadAsAsync<RecipeStepDto>());
        }

        return (draft.Id, steps);
    }

    private async Task<RecipeDetailDto> GetSeededDetailAsync(string slug)
    {
        using var scope = factory.Services.CreateScope();
        var id = await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Recipes.Where(r => r.Slug == slug).Select(r => r.Id).SingleAsync();
        return (await scope.ServiceProvider.GetRequiredService<IRecipeReadRepository>().GetDetailByIdAsync(id, CancellationToken.None))!;
    }
}
