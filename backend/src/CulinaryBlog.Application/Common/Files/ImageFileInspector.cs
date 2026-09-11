namespace CulinaryBlog.Application.Common.Files;

/// <summary>
/// CONS-007 / NFR-SEC-004: xác định định dạng ảnh THỰC SỰ qua magic bytes (không tin Content-Type header hay tên file).
/// Chấp nhận: image/jpeg, image/png, image/webp, image/avif.
/// </summary>
public static class ImageFileInspector
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>Số byte đầu cần đọc để nhận diện mọi định dạng hỗ trợ.</summary>
    public const int HeaderLength = 12;

    public static IReadOnlyCollection<string> AllowedContentTypes { get; } =
        ["image/jpeg", "image/png", "image/webp", "image/avif"];

    public static bool IsAllowedContentType(string? contentType) =>
        contentType is not null && AllowedContentTypes.Contains(contentType.Trim().ToLowerInvariant());

    /// <summary>Trả về (contentType, extension) phát hiện từ header, hoặc null nếu không phải ảnh hợp lệ.</summary>
    public static DetectedImage? Detect(ReadOnlySpan<byte> header)
    {
        // JPEG: FF D8 FF
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return new DetectedImage("image/jpeg", ".jpg");
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return new DetectedImage("image/png", ".png");
        }

        // WebP: "RIFF" ???? "WEBP"
        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
        {
            return new DetectedImage("image/webp", ".webp");
        }

        // AVIF (ISO-BMFF): ???? "ftyp" "avif" | "avis"
        if (header.Length >= 12 && header[4..8].SequenceEqual("ftyp"u8)
            && (header[8..12].SequenceEqual("avif"u8) || header[8..12].SequenceEqual("avis"u8)))
        {
            return new DetectedImage("image/avif", ".avif");
        }

        return null;
    }
}

public sealed record DetectedImage(string ContentType, string Extension);
