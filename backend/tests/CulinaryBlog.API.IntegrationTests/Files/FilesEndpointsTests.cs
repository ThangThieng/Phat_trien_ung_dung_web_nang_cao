using System.Net;
using System.Net.Http.Headers;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Files;

namespace CulinaryBlog.API.IntegrationTests.Files;

/// <summary>
/// FR-FILE-001 / FR-FILE-002 trên MinIO THẬT (Testcontainers): upload, magic bytes (NFR-SEC-004),
/// chặn path traversal và kiểm tra quyền sở hữu. Dùng storage thật thay vì bản giả vì chính đường đi
/// tới S3 – object key do server sinh, bucket tồn tại, URL public trả về – là thứ cần kiểm chứng.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class FilesEndpointsTests(CulinaryBlogApiFactory factory)
{
    [Fact]
    public async Task Upload_AsAuthor_Returns201WithServerGeneratedKey()
    {
        var client = factory.CreateClientAs("Author");

        var response = await client.PostAsync("/api/v1/files/upload", ImageForm(TestImages.Png(), "image/png", "anh-mon-an.png"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.ReadAsAsync<FileUploadResultDto>();
        Assert.Equal("image/png", result.ContentType);
        Assert.StartsWith($"uploads/{TestDataSeeder.AuthorUserId}/", result.Key, StringComparison.Ordinal);
        Assert.EndsWith(".png", result.Key, StringComparison.Ordinal);

        // Tên file do người dùng gửi KHÔNG được xuất hiện trong object key (server tự sinh Guid).
        Assert.DoesNotContain("anh-mon-an", result.Key, StringComparison.OrdinalIgnoreCase);

        // Ảnh vừa upload phải tải về được qua URL public của MinIO.
        using var anonymous = new HttpClient();
        var download = await anonymous.GetAsync(new Uri(result.Url));
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/files/upload", ImageForm(TestImages.Png(), "image/png", "a.png"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>NFR-SEC-004: Content-Type khai báo là ảnh nhưng magic bytes là script PHP → 400.</summary>
    [Fact]
    public async Task Upload_WithFakedContentType_Returns400FileMimeInvalid()
    {
        var client = factory.CreateClientAs("Author");

        var response = await client.PostAsync(
            "/api/v1/files/upload",
            ImageForm(TestImages.NotAnImage(), "image/png", "shell.png"));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.FileMimeInvalid);
    }

    /// <summary>Magic bytes đúng là JPEG nhưng khai báo image/png – hai thứ phải KHỚP nhau, không chỉ "là ảnh".</summary>
    [Fact]
    public async Task Upload_WithMismatchedRealFormat_Returns400FileMimeInvalid()
    {
        var client = factory.CreateClientAs("Author");

        var response = await client.PostAsync(
            "/api/v1/files/upload",
            ImageForm(TestImages.Jpeg(), "image/png", "thuc-te-la-jpeg.png"));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.FileMimeInvalid);
    }

    [Fact]
    public async Task Upload_WithDisallowedContentType_Returns400()
    {
        var client = factory.CreateClientAs("Author");

        var response = await client.PostAsync(
            "/api/v1/files/upload",
            ImageForm(TestImages.Png(), "image/gif", "anh.gif"));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.FileMimeInvalid);
    }

    [Fact]
    public async Task Upload_WebpIsAccepted()
    {
        var client = factory.CreateClientAs("Author");

        var response = await client.PostAsync("/api/v1/files/upload", ImageForm(TestImages.Webp(), "image/webp", "anh.webp"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith(".webp", (await response.ReadAsAsync<FileUploadResultDto>()).Key, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Delete_OwnFile_Returns204AndRemovesObject()
    {
        var client = factory.CreateClientAs("Author");
        var uploaded = await (await client.PostAsync("/api/v1/files/upload", ImageForm(TestImages.Png(), "image/png", "a.png")))
            .ReadAsAsync<FileUploadResultDto>();

        var response = await client.DeleteAsync(new Uri($"/api/v1/files/{uploaded.Key}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var anonymous = new HttpClient();
        var download = await anonymous.GetAsync(new Uri(uploaded.Url));
        Assert.Equal(HttpStatusCode.NotFound, download.StatusCode);
    }

    /// <summary>FR-FILE-002: idempotent – xóa object không tồn tại vẫn 204, không phải 404/500.</summary>
    [Fact]
    public async Task Delete_NonExistentOwnFile_IsIdempotent()
    {
        var client = factory.CreateClientAs("Author");

        var response = await client.DeleteAsync(
            new Uri($"/api/v1/files/uploads/{TestDataSeeder.AuthorUserId}/{Guid.NewGuid()}.png", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>FR-FILE-002 A1: Author khác không xóa được file trong thư mục của tôi.</summary>
    [Fact]
    public async Task Delete_FileOfAnotherAuthor_Returns403()
    {
        var owner = factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId);
        var uploaded = await (await owner.PostAsync("/api/v1/files/upload", ImageForm(TestImages.Png(), "image/png", "a.png")))
            .ReadAsAsync<FileUploadResultDto>();

        var intruder = factory.CreateClientAs("Author", TestDataSeeder.OtherAuthorUserId);
        var response = await intruder.DeleteAsync(new Uri($"/api/v1/files/{uploaded.Key}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, ErrorCodes.FileForbidden);

        // File phải còn nguyên sau khi bị từ chối.
        using var anonymous = new HttpClient();
        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync(new Uri(uploaded.Url))).StatusCode);
    }

    /// <summary>Admin được phép dọn file của bất kỳ ai (SRS §2.3).</summary>
    [Fact]
    public async Task Delete_FileOfAnotherAuthor_AsAdmin_Returns204()
    {
        var owner = factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId);
        var uploaded = await (await owner.PostAsync("/api/v1/files/upload", ImageForm(TestImages.Png(), "image/png", "a.png")))
            .ReadAsAsync<FileUploadResultDto>();

        var admin = factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId);
        var response = await admin.DeleteAsync(new Uri($"/api/v1/files/{uploaded.Key}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// NFR-SEC-004 – path traversal: "uploads/{me}/../{other}/file.png" trông như thư mục của tôi ở ký tự
    /// đầu nhưng trỏ sang người khác. Validator chặn mọi key chứa ".." trước khi tới tầng lưu trữ.
    /// </summary>
    [Theory]
    [InlineData("uploads/{0}/../{1}/anh.png")]
    [InlineData("uploads/{0}/../../etc/passwd")]
    public async Task Delete_WithPathTraversal_Returns400(string keyTemplate)
    {
        var client = factory.CreateClientAs("Author", TestDataSeeder.AuthorUserId);
        var key = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            keyTemplate,
            TestDataSeeder.AuthorUserId,
            TestDataSeeder.OtherAuthorUserId);

        var response = await client.DeleteAsync(new Uri($"/api/v1/files/{Uri.EscapeDataString(key)}", UriKind.Relative));

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, ErrorCodes.ValidationError);
    }

    [Fact]
    public async Task Delete_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync(new Uri("/api/v1/files/uploads/x/y.png", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static MultipartFormDataContent ImageForm(byte[] bytes, string contentType, string fileName)
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent { { content, "file", fileName } };
    }
}
