using System.ComponentModel.DataAnnotations;

namespace CulinaryBlog.Infrastructure.Storage;

/// <summary>SRS §5.3 – MinIO__Endpoint, MinIO__AccessKey, MinIO__SecretKey, MinIO__BucketName.</summary>
public sealed class MinioOptions
{
    public const string SectionName = "MinIO";

    /// <summary>Endpoint S3 API mà backend kết nối (trong Docker: http://minio:9000).</summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>URL công khai trình duyệt dùng để tải ảnh, gồm cả bucket (ví dụ: http://localhost:9000/culinary-blog).</summary>
    [Required]
    public string PublicBaseUrl { get; set; } = string.Empty;

    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian tối đa cho MỘT thao tác với MinIO (tải lên, xóa, đọc), kể cả tra DNS. Đo 19/11/2026: container MinIO dừng thì riêng
    /// việc tra tên "minio" mất ~7 giây, SDK thử lại thêm một lần → người dùng chờ 12 – 16 giây mới thấy 503. Hết giờ → 503
    /// FILE_STORAGE_UNAVAILABLE như lỗi kết nối (FR-FILE-001/002). Ảnh ≤ 5 MB trong cùng mạng Docker chỉ mất vài chục ms.
    /// </summary>
    [Range(1, 300)]
    public int OperationTimeoutSeconds { get; set; } = 5;

    /// <summary>SRS §3.5: bucket "culinary-blog", policy public-read.</summary>
    [Required]
    public string BucketName { get; set; } = "culinary-blog";
}
