using System.Diagnostics;
using Amazon.Runtime;
using Amazon.S3;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.API.IntegrationTests;

/// <summary>
/// NFR-REL-002 (đo 19/11/2026): MinIO không trả lời thì POST /files/upload từng chờ 12 – 16 giây mới trả 503. Mỗi thao tác với MinIO
/// giờ có hạn <see cref="MinioOptions.OperationTimeoutSeconds"/>; hết hạn → 503 FILE_STORAGE_UNAVAILABLE như lỗi kết nối, còn người
/// gọi tự hủy (trình duyệt đóng tab) vẫn là hủy, không biến thành 503.
/// </summary>
public sealed class MinioStorageTimeoutTests
{
    // Địa chỉ không định tuyến được: gói SYN bị bỏ lặng lẽ nên kết nối treo — đúng kiểu MinIO "chết" mà không báo lỗi ngay.
    private const string BlackHoleEndpoint = "http://10.255.255.1:9000";

    [Fact]
    public async Task UploadToAnUnresponsiveStoreFailsWith503WithinTheOperationTimeout()
    {
        using var s3 = CreateClient();
        var storage = CreateStorage(s3, timeoutSeconds: 1);
        using var content = new MemoryStream([1, 2, 3]);

        var watch = Stopwatch.StartNew();
        var error = await Assert.ThrowsAsync<ServiceUnavailableException>(
            () => storage.UploadAsync(content, "uploads/test", ".png", "image/png")).ConfigureAwait(true);
        watch.Stop();

        Assert.Equal(ErrorCodes.FileStorageUnavailable, error.ErrorCode);
        Assert.InRange(watch.Elapsed, TimeSpan.Zero, TimeSpan.FromSeconds(4));
    }

    [Fact]
    public async Task DeleteAndReadAlsoStopAtTheOperationTimeout()
    {
        using var s3 = CreateClient();
        var storage = CreateStorage(s3, timeoutSeconds: 1);

        var watch = Stopwatch.StartNew();
        await Assert.ThrowsAsync<ServiceUnavailableException>(() => storage.DeleteAsync("uploads/test/a.png")).ConfigureAwait(true);
        await Assert.ThrowsAsync<ServiceUnavailableException>(() => storage.OpenReadAsync("uploads/test/a.png")).ConfigureAwait(true);
        watch.Stop();

        Assert.InRange(watch.Elapsed, TimeSpan.Zero, TimeSpan.FromSeconds(8));
    }

    [Fact]
    public async Task CallerCancellationStaysACancellationNotA503()
    {
        using var s3 = CreateClient();
        var storage = CreateStorage(s3, timeoutSeconds: 30);
        using var content = new MemoryStream([1, 2, 3]);
        using var caller = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => storage.UploadAsync(content, "uploads/test", ".png", "image/png", caller.Token)).ConfigureAwait(true);
    }

    private static AmazonS3Client CreateClient() =>
        new(
            new BasicAWSCredentials("test", "test-secret"),
            new AmazonS3Config
            {
                ServiceURL = BlackHoleEndpoint,
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
                MaxErrorRetry = 1,
                Timeout = TimeSpan.FromSeconds(10),
            });

    private static MinioFileStorageService CreateStorage(IAmazonS3 s3, int timeoutSeconds) =>
        new(
            s3,
            Options.Create(new MinioOptions
            {
                Endpoint = BlackHoleEndpoint,
                PublicBaseUrl = "http://localhost/media",
                AccessKey = "test",
                SecretKey = "test-secret",
                OperationTimeoutSeconds = timeoutSeconds,
            }),
            NullLogger<MinioFileStorageService>.Instance);
}
