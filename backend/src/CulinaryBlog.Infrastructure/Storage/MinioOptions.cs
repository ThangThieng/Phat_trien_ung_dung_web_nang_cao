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

    /// <summary>SRS §3.5: bucket "culinary-blog", policy public-read.</summary>
    [Required]
    public string BucketName { get; set; } = "culinary-blog";
}
