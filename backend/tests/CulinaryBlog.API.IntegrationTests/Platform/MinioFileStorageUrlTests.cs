using CulinaryBlog.API.IntegrationTests.Infrastructure;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.API.IntegrationTests;

[Collection(IntegrationTestSuite.Name)]
public sealed class MinioFileStorageUrlTests(CulinaryBlogApiFactory factory)
{
    [Fact]
    public void StoredFileUrlAcceptsOnlyConfiguredOriginAndSafeObjectPath()
    {
        using var scope = factory.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var publicBaseUrl = scope.ServiceProvider.GetRequiredService<IOptions<MinioOptions>>().Value.PublicBaseUrl.TrimEnd('/');

        Assert.True(storage.IsStoredFileUrl($"{publicBaseUrl}/avatars/photo.png"));
        Assert.False(storage.IsStoredFileUrl("https://cdn.example.invalid/avatars/photo.png"));
        Assert.False(storage.IsStoredFileUrl($"{publicBaseUrl}/avatars/%2e%2e/secrets.txt"));

        var configured = new Uri(publicBaseUrl, UriKind.Absolute);
        var withUserInfo = new UriBuilder(configured)
        {
            UserName = "attacker",
            Password = "password",
            Path = configured.AbsolutePath.TrimEnd('/') + "/avatars/photo.png",
        };
        Assert.False(storage.IsStoredFileUrl(withUserInfo.Uri.AbsoluteUri));
    }
}
