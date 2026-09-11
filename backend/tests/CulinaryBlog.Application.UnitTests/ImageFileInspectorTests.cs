using CulinaryBlog.Application.Common.Files;

namespace CulinaryBlog.Application.UnitTests;

public class ImageFileInspectorTests
{
    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0 }, "image/jpeg", ".jpg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 }, "image/png", ".png")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 1, 2, 3, 4, 0x57, 0x45, 0x42, 0x50 }, "image/webp", ".webp")]
    [InlineData(new byte[] { 0, 0, 0, 0x1C, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66 }, "image/avif", ".avif")]
    public void Detect_RecognisesSupportedFormats(byte[] header, string contentType, string extension)
    {
        var detected = ImageFileInspector.Detect(header);

        Assert.NotNull(detected);
        Assert.Equal(contentType, detected.ContentType);
        Assert.Equal(extension, detected.Extension);
    }

    [Theory]
    [InlineData(new byte[] { 0x4D, 0x5A, 0x90, 0x00 })] // Windows PE (exe) giả dạng ảnh
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })] // GIF – không nằm trong whitelist
    [InlineData(new byte[] { })]
    public void Detect_RejectsUnsupportedOrForgedFiles(byte[] header) =>
        Assert.Null(ImageFileInspector.Detect(header));

    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("IMAGE/PNG", true)]
    [InlineData("image/gif", false)]
    [InlineData("application/octet-stream", false)]
    [InlineData(null, false)]
    public void IsAllowedContentType_UsesWhitelist(string? contentType, bool expected) =>
        Assert.Equal(expected, ImageFileInspector.IsAllowedContentType(contentType));
}
