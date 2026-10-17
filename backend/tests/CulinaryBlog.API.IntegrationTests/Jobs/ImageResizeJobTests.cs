using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.API.IntegrationTests.Content;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CulinaryBlog.API.IntegrationTests.Jobs;

/// <summary>
/// Buổi 4 — Dev 4: FR-JOB-002 Image Resize Job với ảnh JPEG THẬT (sinh bằng ImageSharp trong test — ảnh nhị phân không
/// nằm trong git) trên MinIO thật: tải ảnh lên → job được xếp hàng → chạy job → đủ 3 biến thể đúng kích thước.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class ImageResizeJobTests(CulinaryBlogApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UploadThenRunJob_CreatesMedium800x600AndThumbnail300x300()
    {
        var author = factory.CreateClientAs("Author");
        var recipe = await RecipeApi.CreateDraftAsync(author, "Bánh xèo miền Tây");
        var uploaded = await RecipeApi.UploadImageAsync(author, recipe.Id, JpegPhoto(1600, 1000), "image/jpeg", "banh-xeo.jpg");

        Assert.Null(uploaded.MediumUrl);
        Assert.Null(uploaded.ThumbnailUrl);
        Assert.True(
            factory.BackgroundJobs.WasEnqueued<ImageResizeJob>(nameof(ImageResizeJob.ExecuteAsync), uploaded.ImageId),
            "Tải ảnh lên nhưng không xếp hàng ImageResizeJob.");

        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ImageResizeJob>().ExecuteAsync(uploaded.ImageId, CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var image = await db.RecipeImages.AsNoTracking().SingleAsync(i => i.Id == uploaded.ImageId);
        Assert.NotNull(image.MediumUrl);
        Assert.NotNull(image.ThumbnailUrl);
        Assert.Contains($"/recipes/{recipe.Id}/", image.MediumUrl, StringComparison.Ordinal);

        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        Assert.Equal(new Size(800, 600), await DimensionsAsync(s3, image.MediumUrl));
        Assert.Equal(new Size(300, 300), await DimensionsAsync(s3, image.ThumbnailUrl));
        Assert.True(await MinioObject.ExistsAsync(s3, image.OriginalUrl), "Ảnh gốc phải được giữ nguyên.");
    }

    /// <summary>
    /// Buổi 5 — Dev 4 (chuyển từ Buổi 4): card danh sách (<c>RecipeSummaryDto.primaryImageUrl</c>) dùng ThumbnailUrl 300×300,
    /// và dự phòng OriginalUrl khi job chưa chạy xong — card không bao giờ trống ảnh chỉ vì job còn trong hàng đợi.
    /// Đọc qua <c>/recipes/mine</c> (no-store) để không dính cache <c>recipes:list</c> 2 phút của danh sách công khai.
    /// </summary>
    [Fact]
    public async Task RecipeCard_UsesOriginalUntilJobRuns_ThenThumbnail()
    {
        var author = factory.CreateClientAs("Author");
        var recipe = await RecipeApi.CreateDraftAsync(author, "Gỏi cuốn tôm thịt");
        var uploaded = await RecipeApi.UploadImageAsync(author, recipe.Id, JpegPhoto(1200, 900), "image/jpeg", "goi-cuon.jpg");
        Assert.True(uploaded.IsPrimary, "Ảnh đầu tiên của công thức phải tự thành ảnh chính.");

        Assert.Equal(uploaded.OriginalUrl, await CardImageUrlAsync(author, recipe.Id));

        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ImageResizeJob>().ExecuteAsync(uploaded.ImageId, CancellationToken.None);
        var image = await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>()
            .RecipeImages.AsNoTracking().SingleAsync(i => i.Id == uploaded.ImageId);

        Assert.NotNull(image.ThumbnailUrl);
        Assert.Equal(image.ThumbnailUrl, await CardImageUrlAsync(author, recipe.Id));
    }

    /// <summary>Ảnh bị xóa trước khi job chạy → job kết thúc êm, không ném lỗi (không có gì để thử lại).</summary>
    [Fact]
    public async Task Run_ForDeletedImage_CompletesWithoutError()
    {
        using var scope = factory.Services.CreateScope();

        var exception = await Record.ExceptionAsync(() =>
            scope.ServiceProvider.GetRequiredService<ImageResizeJob>().ExecuteAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Null(exception);
    }

    /// <summary>
    /// Lớp chặn thứ hai cho các advisory TIFF/BigTIFF chưa vá của ImageSharp 3.x (Directory.Build.props): kể cả khi object
    /// gốc trên MinIO bị thay bằng một TIFF hợp lệ (vượt qua kiểm tra magic bytes lúc tải lên), job không giải mã nó.
    /// </summary>
    [Fact]
    public async Task Run_WhenOriginalIsReplacedByTiff_DoesNotDecodeIt_AndKeepsOriginal()
    {
        var author = factory.CreateClientAs("Author");
        var recipe = await RecipeApi.CreateDraftAsync(author, "Chả giò rế");
        var uploaded = await RecipeApi.UploadImageAsync(author, recipe.Id, JpegPhoto(640, 480), "image/jpeg", "cha-gio.jpg");

        var tiff = TiffPhoto(640, 480);
        using (var check = Image.Load(tiff))
        {
            Assert.Equal(640, check.Width); // cấu hình mặc định của ImageSharp đọc được file này — đúng là TIFF hợp lệ
        }

        using var scope = factory.Services.CreateScope();
        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        var (bucket, key) = MinioObject.Parse(uploaded.OriginalUrl);
        using (var body = new MemoryStream(tiff))
        {
            await s3.PutObjectAsync(new PutObjectRequest { BucketName = bucket, Key = key, InputStream = body, ContentType = "image/jpeg" });
        }

        var exception = await Record.ExceptionAsync(() =>
            scope.ServiceProvider.GetRequiredService<ImageResizeJob>().ExecuteAsync(uploaded.ImageId, CancellationToken.None));

        Assert.Null(exception);
        var image = await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>()
            .RecipeImages.AsNoTracking().SingleAsync(i => i.Id == uploaded.ImageId);
        Assert.Null(image.MediumUrl);
        Assert.Null(image.ThumbnailUrl);
        Assert.True(await MinioObject.ExistsAsync(s3, image.OriginalUrl), "Ảnh gốc phải được giữ nguyên.");
    }

    private static async Task<string?> CardImageUrlAsync(HttpClient client, Guid recipeId)
    {
        var response = await client.GetAsync(new Uri("/api/v1/recipes/mine?pageSize=50", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.ReadAsAsync<PagedResult<RecipeSummaryDto>>();
        return page.Items.Single(r => r.Id == recipeId).PrimaryImageUrl;
    }

    private static async Task<Size> DimensionsAsync(IAmazonS3 s3, string url)
    {
        var (bucket, key) = MinioObject.Parse(url);
        using var response = await s3.GetObjectAsync(bucket, key);
        using var image = await Image.LoadAsync(response.ResponseStream);
        return image.Size;
    }

    private static byte[] JpegPhoto(int width, int height)
    {
        using var image = Photo(width, height);
        using var output = new MemoryStream();
        image.SaveAsJpeg(output);
        return output.ToArray();
    }

    private static byte[] TiffPhoto(int width, int height)
    {
        using var image = Photo(width, height);
        using var output = new MemoryStream();
        image.SaveAsTiff(output);
        return output.ToArray();
    }

    private static Image<Rgb24> Photo(int width, int height)
    {
        var image = new Image<Rgb24>(width, height);
        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
            {
                var row = rows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    row[x] = new Rgb24((byte)(x % 256), (byte)(y % 256), 128);
                }
            }
        });

        return image;
    }
}
