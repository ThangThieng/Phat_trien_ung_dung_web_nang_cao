using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Util;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Storage;

/// <summary>
/// FR-OBS-001 — kiểm tra MinIO bằng cách hỏi bucket mà ứng dụng thực sự dùng có tồn tại không.
/// Tự viết vì package <c>AspNetCore.HealthChecks.Minio</c> mà SRS nêu không tồn tại trên NuGet
/// (CONG_NGHE_VA_PHIEN_BAN.md §6.4). Kiểm tra đúng bucket cấu hình (không chỉ ListBuckets) để phát hiện cả trường hợp
/// MinIO sống nhưng bucket bị xóa — lúc đó upload vẫn lỗi.
/// Được đăng ký với failureStatus = Degraded và KHÔNG gắn tag "ready": mất MinIO chỉ làm ảnh không tải lên được,
/// công thức vẫn đọc/ghi được — là suy giảm, không phải ngừng phục vụ.
/// </summary>
public sealed class MinioHealthCheck(IAmazonS3 s3, IOptions<MinioOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var bucket = options.Value.BucketName;

        try
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucket).WaitAsync(cancellationToken).ConfigureAwait(false);
            return exists
                ? HealthCheckResult.Healthy($"Bucket '{bucket}' sẵn sàng.")
                : new HealthCheckResult(context.Registration.FailureStatus, $"Bucket '{bucket}' không tồn tại.");
        }
        catch (Exception ex) when (ex is AmazonServiceException or AmazonClientException or HttpRequestException or TimeoutException or OperationCanceledException)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Không kết nối được MinIO.", ex);
        }
    }
}
