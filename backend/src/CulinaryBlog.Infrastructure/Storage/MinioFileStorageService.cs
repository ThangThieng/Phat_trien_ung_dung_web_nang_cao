using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
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

        using var timeout = StartTimeout(cancellationToken);
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
                timeout.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsUnavailable(ex, cancellationToken))
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
        using var timeout = StartTimeout(cancellationToken);
        try
        {
            // S3 DeleteObject idempotent: object không tồn tại vẫn trả 204
            await s3.DeleteObjectAsync(_options.BucketName, key, timeout.Token).ConfigureAwait(false);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Idempotent – bỏ qua
        }
        catch (Exception ex) when (IsUnavailable(ex, cancellationToken))
        {
            LogStorageError(logger, "delete", key, ex);
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");
        }
    }

    public async Task<Stream> OpenReadAsync(string fileUrlOrKey, CancellationToken cancellationToken = default)
    {
        var key = ToObjectKey(fileUrlOrKey);
        using var timeout = StartTimeout(cancellationToken);
        try
        {
            using var response = await s3.GetObjectAsync(_options.BucketName, key, timeout.Token).ConfigureAwait(false);
            var buffer = new MemoryStream();
            await using (response.ResponseStream.ConfigureAwait(false))
            {
                await response.ResponseStream.CopyToAsync(buffer, timeout.Token).ConfigureAwait(false);
            }

            buffer.Position = 0;
            return buffer;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"Không tìm thấy tệp '{key}' trên object storage.", key, ex);
        }
        catch (Exception ex) when (IsUnavailable(ex, cancellationToken))
        {
            LogStorageError(logger, "read", key, ex);
            throw new ServiceUnavailableException(ErrorCodes.FileStorageUnavailable, "Dịch vụ lưu trữ ảnh tạm thời không khả dụng.");
        }
    }

    public bool IsStoredFileUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        var prefix = _options.PublicBaseUrl.TrimEnd('/') + "/";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var file) ||
            !Uri.TryCreate(prefix, UriKind.Absolute, out var bucket))
        {
            return false;
        }

        // Uri normalizes dot segments (including percent-encoded variants) before exposing AbsolutePath.
        // Inspect the caller-supplied representation too, otherwise /bucket/a/%2e%2e/b can look harmless after parsing.
        var hasDotSegment = Uri.UnescapeDataString(url)
            .Split('/', StringSplitOptions.None)
            .Any(segment => segment is "." or "..");

        return file.Scheme is "http" or "https" && file.Scheme == bucket.Scheme &&
            string.Equals(file.IdnHost, bucket.IdnHost, StringComparison.OrdinalIgnoreCase) &&
            file.Port == bucket.Port && string.IsNullOrEmpty(file.UserInfo) &&
            file.AbsolutePath.StartsWith(bucket.AbsolutePath, StringComparison.Ordinal) &&
            file.AbsolutePath.Length > bucket.AbsolutePath.Length &&
            !hasDotSegment;
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

    /// <summary>Lỗi kết nối/phía MinIO, hoặc hết <see cref="MinioOptions.OperationTimeoutSeconds"/> (không phải người gọi tự hủy).</summary>
    private static bool IsUnavailable(Exception ex, CancellationToken callerToken) =>
        ex is AmazonS3Exception or HttpRequestException or AmazonServiceException ||
        (ex is OperationCanceledException && !callerToken.IsCancellationRequested);

    private CancellationTokenSource StartTimeout(CancellationToken callerToken)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(callerToken);
        source.CancelAfter(TimeSpan.FromSeconds(_options.OperationTimeoutSeconds));
        return source;
    }
}
