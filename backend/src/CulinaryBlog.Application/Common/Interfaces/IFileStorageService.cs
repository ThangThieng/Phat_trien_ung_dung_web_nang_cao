namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// FR-FILE-001/002 – abstraction lưu trữ đối tượng (MinIO ↔ AWS S3 ↔ local) để Application không phụ thuộc hạ tầng.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Upload vào "{folder}/{Guid}{extension}" (tên file do server sinh – chống path traversal).</summary>
    Task<StoredFile> UploadAsync(Stream content, string folder, string extension, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Xóa theo public URL hoặc object key. Idempotent: object không tồn tại không ném lỗi.</summary>
    Task DeleteAsync(string fileUrlOrKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đọc nội dung tệp theo public URL hoặc object key (FR-JOB-002 đọc ảnh gốc để đổi kích thước). Trả stream đã nạp
    /// vào bộ nhớ — ảnh đã bị giới hạn ≤ 5MB lúc tải lên; người gọi dispose. Tệp không tồn tại → FileNotFoundException.
    /// </summary>
    Task<Stream> OpenReadAsync(string fileUrlOrKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL có trỏ vào đúng bucket công khai của hệ thống không (FR-AUTH-007: <c>avatarUrl</c> phải là tệp đã tải lên qua
    /// <c>POST /files/upload</c>, không phải ảnh ở domain ngoài — chặn hotlink và theo dõi người xem qua ảnh bên thứ ba).
    /// </summary>
    bool IsStoredFileUrl(string url);
}

public sealed record StoredFile(string Key, string Url);
