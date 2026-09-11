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
}

public sealed record StoredFile(string Key, string Url);
