using System.Reflection;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.ArchitectureTests;

/// <summary>
/// Buổi 8 — Dev 4: hai quy tắc mà log và cache dựa vào nhưng trình biên dịch không tự kiểm.
/// </summary>
public class ObservabilityRulesTests
{
    private static readonly Assembly ApplicationAssembly = Assembly.Load("CulinaryBlog.Application");

    /// <summary>
    /// SRS NFR-SEC-006 + MT-34: chỉ query của endpoint CÔNG KHAI được cache dùng chung. Danh sách đóng: thêm query mới vào đây là
    /// một quyết định có chủ đích (kèm dòng trong bảng TTL NFR-PERF-003), không phải tác dụng phụ của việc lỡ cài
    /// <see cref="ICacheable"/> cho <c>GET /recipes/mine</c>, <c>/auth/sessions</c> hay <c>/users</c>.
    /// </summary>
    private static readonly string[] PublicCacheableQueries =
    [
        "GetCategoriesQuery",
        "GetCategoryBySlugQuery",
        "GetRecipesQuery",
        "GetRecipeBySlugQuery",
        "GetRecipeSitemapQuery",
        "SearchRecipesQuery",
    ];

    /// <summary>
    /// <c>LoggingBehavior</c> ghi audit (NFR-SEC-006) cho request có tên kết thúc bằng "Command" và log thường cho "Query". Request
    /// đặt tên khác sẽ lặng lẽ rơi khỏi audit — chặn ở đây.
    /// </summary>
    [Fact]
    public void EveryMediatRRequest_IsNamedCommandOrQuery()
    {
        var offenders = RequestTypes()
            .Where(t => !t.Name.EndsWith("Command", StringComparison.Ordinal) && !t.Name.EndsWith("Query", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ToList();

        Assert.True(offenders.Count == 0, "Request MediatR phải tên *Command (ghi — có audit) hoặc *Query (đọc): " + string.Join(", ", offenders));
        Assert.Contains(RequestTypes(), t => t.Name == "LoginUserCommand");
    }

    [Fact]
    public void OnlyPublicQueries_ImplementICacheable()
    {
        var cacheable = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICacheable).IsAssignableFrom(t))
            .Select(t => t.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(PublicCacheableQueries.Order(StringComparer.Ordinal), cacheable);
    }

    [Fact]
    public void CommandsNeverImplementICacheable_AndQueriesNeverInvalidate()
    {
        foreach (var type in RequestTypes())
        {
            if (type.Name.EndsWith("Command", StringComparison.Ordinal))
            {
                Assert.False(typeof(ICacheable).IsAssignableFrom(type), $"{type.Name} là command nhưng lại được cache.");
            }
            else
            {
                Assert.False(typeof(ICacheInvalidator).IsAssignableFrom(type), $"{type.Name} là query nhưng lại xóa cache.");
            }
        }
    }

    private static IEnumerable<Type> RequestTypes() =>
        ApplicationAssembly.GetTypes().Where(t =>
            t is { IsClass: true, IsAbstract: false }
            && t.GetInterfaces().Any(i => i == typeof(IRequest) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))));
}
