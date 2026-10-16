namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// Một collection duy nhất cho toàn bộ integration test: ba container Testcontainers mất vài giây để
/// khởi động, nên chúng được dựng MỘT lần và dùng chung. Đánh đổi là các lớp test chạy tuần tự —
/// chấp nhận được vì tổng thời gian vẫn thấp hơn nhiều so với dựng container cho từng lớp.
/// Lớp test nào ghi dữ liệu thì gọi <c>Factory.ResetDatabaseAsync()</c> để tự cô lập.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationTestSuite : ICollectionFixture<CulinaryBlogApiFactory>
{
    public const string Name = "culinary-blog-integration";
}
