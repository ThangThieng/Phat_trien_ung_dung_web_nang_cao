using System.Net;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.IntegrationTests.Infrastructure;

namespace CulinaryBlog.API.IntegrationTests.Jobs;

/// <summary>
/// FR-JOB-001 / MT-39 – lớp bảo vệ thứ hai của Hangfire Dashboard. Basic Auth nằm ở Nginx nên không
/// kiểm chứng được từ đây; cái kiểm chứng được — và cũng là cái dễ hỏng âm thầm nhất — là API tự nó
/// từ chối mọi request không mang header do Nginx gắn, kể cả request đến thẳng cổng 5000 của container api.
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public class HangfireDashboardTests(CulinaryBlogApiFactory factory)
{
    [Fact]
    public async Task Dashboard_WithoutGateHeader_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/hangfire", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// JWT Admin của ứng dụng KHÔNG mở được dashboard — đúng quyết định MT-39: dashboard nằm ngoài hệ
    /// thống phân quyền Bearer. Hangfire trả 403 (chứ không phải 401) khi request ĐÃ xác thực nhưng bị
    /// filter từ chối; điều cần khẳng định là "không vào được", không phải con số cụ thể.
    /// </summary>
    [Fact]
    public async Task Dashboard_WithAdminJwtButNoGateHeader_IsRejected()
    {
        var client = factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId);

        var response = await client.GetAsync(new Uri("/hangfire", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_WithWrongGateSecret_Returns401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(NginxGateDashboardFilter.HeaderName, "secret-sai");

        var response = await client.GetAsync(new Uri("/hangfire", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_WithGateSecretFromNginx_Returns200()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(NginxGateDashboardFilter.HeaderName, TestDataSeeder.HangfireGateSecret);

        var response = await client.GetAsync(new Uri("/hangfire", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
