using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Storage;

/// <summary>Khi khởi động: tạo bucket (nếu chưa có) và đặt policy public-read cho object (SRS §3.5).</summary>
public sealed partial class MinioBucketInitializer(
    IAmazonS3 s3,
    IOptions<MinioOptions> options,
    ILogger<MinioBucketInitializer> logger) : BackgroundService
{
    private const int MaxAttempts = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bucket = options.Value.BucketName;

        for (var attempt = 1; attempt <= MaxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucket).ConfigureAwait(false))
                {
                    await s3.PutBucketAsync(new PutBucketRequest { BucketName = bucket }, stoppingToken).ConfigureAwait(false);
                }

                await s3.PutBucketPolicyAsync(
                    new PutBucketPolicyRequest { BucketName = bucket, Policy = PublicReadPolicy(bucket) },
                    stoppingToken).ConfigureAwait(false);

                LogReady(logger, bucket);
                return;
            }
            catch (Exception ex) when (ex is AmazonS3Exception or HttpRequestException or AmazonServiceException)
            {
                LogRetry(logger, bucket, attempt, MaxAttempts, ex);
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private static string PublicReadPolicy(string bucket) =>
        $$"""
        {
          "Version": "2012-10-17",
          "Statement": [
            {
              "Effect": "Allow",
              "Principal": { "AWS": ["*"] },
              "Action": ["s3:GetObject"],
              "Resource": ["arn:aws:s3:::{{bucket}}/*"]
            }
          ]
        }
        """;

    [LoggerMessage(Level = LogLevel.Information, Message = "MinIO bucket {Bucket} is ready (public-read)")]
    private static partial void LogReady(ILogger logger, string bucket);

    [LoggerMessage(Level = LogLevel.Warning, Message = "MinIO bucket {Bucket} init failed (attempt {Attempt}/{MaxAttempts})")]
    private static partial void LogRetry(ILogger logger, string bucket, int attempt, int maxAttempts, Exception exception);
}
