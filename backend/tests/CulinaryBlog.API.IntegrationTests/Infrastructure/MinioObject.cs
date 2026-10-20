using System.Net;
using Amazon.S3;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>Tách URL public của MinIO (<c>{endpoint}/{bucket}/{key}</c>) và hỏi thẳng S3 xem object còn tồn tại không.</summary>
public static class MinioObject
{
    public static (string Bucket, string Key) Parse(string url)
    {
        var path = new Uri(url).AbsolutePath.TrimStart('/');
        var slash = path.IndexOf('/', StringComparison.Ordinal);
        return (path[..slash], Uri.UnescapeDataString(path[(slash + 1)..]));
    }

    public static async Task<bool> ExistsAsync(IAmazonS3 s3, string url)
    {
        ArgumentNullException.ThrowIfNull(s3);

        var (bucket, key) = Parse(url);
        try
        {
            await s3.GetObjectMetadataAsync(bucket, key).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
