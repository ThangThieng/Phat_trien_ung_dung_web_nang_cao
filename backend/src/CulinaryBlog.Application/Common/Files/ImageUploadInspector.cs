using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.Application.Common.Files;

/// <summary>
/// Ba lớp kiểm tra bắt buộc của MỌI lần tải ảnh lên (NFR-SEC-004, CONS-007) — một chỗ cho cả <c>POST /files/upload</c>
/// (FR-FILE-001) lẫn <c>POST /recipes/{id}/images</c> (FR-RCP-008): (1) kích thước ≤ 5MB trước khi đọc stream,
/// (2) MIME khai báo nằm trong danh sách cho phép, (3) magic bytes khớp MIME khai báo.
/// </summary>
public static class ImageUploadInspector
{
    /// <summary>Kiểm tra ảnh; sai ở lớp nào thì ném BadRequestException với mã FILE_* tương ứng (400).</summary>
    /// <returns>Định dạng thật và stream đầy đủ (header đã đọc + phần còn lại, không buffer cả file vào bộ nhớ).</returns>
    public static async Task<InspectedImage> InspectAsync(
        Stream content,
        long length,
        string? declaredContentType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        // File rỗng không phải "file quá lớn" (review Buổi 3 — Dev 2, mục 16): nó không phải ảnh hợp lệ → FILE_MIME_INVALID.
        if (length <= 0)
        {
            throw new BadRequestException(ErrorCodes.FileMimeInvalid, "File rỗng — không phải ảnh hợp lệ.");
        }

        if (length > ImageFileInspector.MaxFileSizeBytes)
        {
            throw new BadRequestException(ErrorCodes.FileSizeExceeded, "Kích thước file vượt quá giới hạn 5MB.");
        }

        if (!ImageFileInspector.IsAllowedContentType(declaredContentType))
        {
            throw new BadRequestException(ErrorCodes.FileMimeInvalid, "Loại file không được phép. Chỉ chấp nhận JPEG, PNG, WebP, AVIF.");
        }

        var header = new byte[ImageFileInspector.HeaderLength];
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);
        var detected = ImageFileInspector.Detect(header.AsSpan(0, read));

        if (detected is null || !string.Equals(detected.ContentType, declaredContentType!.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(ErrorCodes.FileMimeInvalid, "File không hợp lệ.");
        }

        return new InspectedImage(detected, new PrefixedReadStream(header.AsMemory(0, read), content, length));
    }
}

public sealed record InspectedImage(DetectedImage Format, Stream Content);
