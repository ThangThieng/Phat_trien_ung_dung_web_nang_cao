using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Storage;

/// <summary>FR-FILE-001/002 – IFileStorageService trên MinIO qua AWSSDK.S3 (endpoint override, path-style).</summary>
public sealed partial class MinioFileStorageService(
    IAmazonS3 s3,
    IOptions<MinioOptions> options,
    ILogger<MinioFileStorageService> logger) : IFileStorageService
{
    private readonly MinioOptions _options = options.Value;

    public async Task<StoredFile> UploadAsync(Stream content, string folder, string extension, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        // Tên file do server sinh: {folder}/{Guid}{ext} – không dùng tên file của user (chống path traversal)
        var key = $"{folder.Trim('/')}/{Guid.NewGuid():N}{extension}";

        // SigV4 cần tính hash payload → stream phải seekable. File đã được giới hạn ≤ 5MB trước khi tới đây.
        var seekable = content;
        MemoryStream? buffer = null;
        if (!content.CanSeek)
        {
            buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            buffer.Position = 0;
            seekable = buffer;
        }

        try
        {
            await s3.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    InputStream = seekable,
                    ContentType = contentType,
                    AutoCloseStream = false,
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is AmazonS3Exception or HttpRequestException or AmazonServiceException)
        {
            LogStorageError(logger, "upload", key, ex);
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");
        }
        finally
        {
            if (buffer is not null)
            {
                await buffer.DisposeAsync().ConfigureAwait(false);
            }
        }

        return new StoredFile(key, $"{_options.PublicBaseUrl.TrimEnd('/')}/{key}");
    }

    public async Task DeleteAsync(string fileUrlOrKey, CancellationToken cancellationToken = default)
    {
        var key = ToObjectKey(fileUrlOrKey);
        try
        {
            // S3 DeleteObject idempotent: object không tồn tại vẫn trả 204
            await s3.DeleteObjectAsync(_options.BucketName, key, cancellationToken).ConfigureAwait(false);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Idempotent – bỏ qua
        }
        catch (Exception ex) when (ex is AmazonS3Exception or HttpRequestException or AmazonServiceException)
        {
            LogStorageError(logger, "delete", key, ex);
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");
        }
    }

    /// <summary>FR-FILE-002: "trích xuất object name từ URL".</summary>
    private string ToObjectKey(string fileUrlOrKey)
    {
        var prefix = _options.PublicBaseUrl.TrimEnd('/') + "/";
        return fileUrlOrKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? fileUrlOrKey[prefix.Length..]
            : fileUrlOrKey.TrimStart('/');
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "MinIO {Operation} failed for object {ObjectKey}")]
    private static partial void LogStorageError(ILogger logger, string operation, string objectKey, Exception exception);
}
