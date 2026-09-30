using System.Text.RegularExpressions;
using CulinaryBlog.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.IntegrationTests.Platform;

/// <summary>
/// Buổi 4 — Dev 4: biến yêu cầu của giảng viên "hoàn thành việc cài đặt tất cả API endpoints" thành điều kiện KIỂM ĐƯỢC.
/// Đọc route THẬT của ứng dụng từ <see cref="EndpointDataSource"/> rồi so với 45 endpoint của SRS v1.2.2 Chương 8
/// (42 dưới <c>/api/v1</c> + 3 health). Đỏ khi THIẾU một endpoint của SRS, và cả khi THỪA một endpoint <c>/api/v1</c> nằm
/// ngoài SRS — endpoint không truy vết được về yêu cầu nào phải đi qua Change Request trước (nguyên tắc §4.2 của kế hoạch).
/// So cấu trúc route, không so tên tham số (<c>{id:guid}</c> ≡ <c>{id}</c>, <c>{ingId}</c> ≡ <c>{ingredientId}</c>).
/// </summary>
[Collection(IntegrationTestSuite.Name)]
public partial class ApiSurfaceTests(CulinaryBlogApiFactory factory)
{
    /// <summary>SRS v1.2.2 Chương 8 — chép nguyên bảng, kèm số mục để truy vết.</summary>
    private static readonly (string Method, string Route, string Srs)[] SrsEndpoints =
    [

        // §8.1 Authentication (12)
        ("POST", "/api/v1/auth/register", "8.1"),
        ("POST", "/api/v1/auth/login", "8.1"),
        ("POST", "/api/v1/auth/google", "8.1"),
        ("POST", "/api/v1/auth/refresh", "8.1"),
        ("POST", "/api/v1/auth/logout", "8.1"),
        ("GET", "/api/v1/auth/me", "8.1"),
        ("PATCH", "/api/v1/auth/me", "8.1"),
        ("GET", "/api/v1/users", "8.1"),
        ("PATCH", "/api/v1/users/{id}/status", "8.1"),
        ("GET", "/api/v1/auth/sessions", "8.1"),
        ("DELETE", "/api/v1/auth/sessions/{id}", "8.1"),
        ("POST", "/api/v1/auth/sessions/revoke-all", "8.1"),

        // §8.2 Categories (5)
        ("GET", "/api/v1/categories", "8.2"),
        ("GET", "/api/v1/categories/{slug}", "8.2"),
        ("POST", "/api/v1/categories", "8.2"),
        ("PUT", "/api/v1/categories/{id}", "8.2"),
        ("DELETE", "/api/v1/categories/{id}", "8.2"),

        // §8.3 Recipes (13)
        ("GET", "/api/v1/recipes", "8.3"),
        ("GET", "/api/v1/recipes/mine", "8.3"),
        ("GET", "/api/v1/recipes/mine/{id}", "8.3"),
        ("GET", "/api/v1/recipes/search", "8.3"),
        ("GET", "/api/v1/recipes/sitemap", "8.3"),
        ("GET", "/api/v1/recipes/{slug}", "8.3"),
        ("POST", "/api/v1/recipes", "8.3"),
        ("PUT", "/api/v1/recipes/{id}", "8.3"),
        ("PATCH", "/api/v1/recipes/{id}/publish", "8.3"),
        ("PATCH", "/api/v1/recipes/{id}/unpublish", "8.3"),
        ("PATCH", "/api/v1/recipes/{id}/archive", "8.3"),
        ("PATCH", "/api/v1/recipes/{id}/unarchive", "8.3"),
        ("DELETE", "/api/v1/recipes/{id}", "8.3"),

        // §8.4 Recipe Images (3)
        ("POST", "/api/v1/recipes/{id}/images", "8.4"),
        ("PATCH", "/api/v1/recipes/{id}/images/{imageId}", "8.4"),
        ("DELETE", "/api/v1/recipes/{id}/images/{imageId}", "8.4"),

        // §8.5 Recipe Steps (4)
        ("POST", "/api/v1/recipes/{id}/steps", "8.5"),
        ("PUT", "/api/v1/recipes/{id}/steps/{stepId}", "8.5"),
        ("PATCH", "/api/v1/recipes/{id}/steps/reorder", "8.5"),
        ("DELETE", "/api/v1/recipes/{id}/steps/{stepId}", "8.5"),

        // §8.6 Recipe Ingredients (3)
        ("POST", "/api/v1/recipes/{id}/ingredients", "8.6"),
        ("PUT", "/api/v1/recipes/{id}/ingredients/{ingId}", "8.6"),
        ("DELETE", "/api/v1/recipes/{id}/ingredients/{ingId}", "8.6"),

        // §8.8 Files (2)
        ("POST", "/api/v1/files/upload", "8.8"),
        ("DELETE", "/api/v1/files/{**key}", "8.8"),

        // §8.7 Health (3) — ngoài tiền tố /api/v1
        ("GET", "/health", "8.7"),
        ("GET", "/health/live", "8.7"),
        ("GET", "/health/ready", "8.7"),
    ];

    /// <summary>
    /// Endpoint của SRS mà chủ API chưa merge vào nhánh này — mỗi dòng ghi rõ chủ API và nhánh sẽ mang nó vào.
    /// Test có chiều ngược lại (<see cref="PendingEndpoints_AreReallyMissing"/>): nhánh của chủ API merge vào mà quên xóa dòng
    /// ở đây thì test đỏ, nên danh sách này không thể thành chỗ "giấu" endpoint thiếu.
    /// </summary>
    private static readonly (string Method, string Route, string Owner)[] PendingEndpoints =
    [
        ("POST", "/api/v1/auth/refresh", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-004)"),
        ("GET", "/api/v1/auth/me", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-006)"),
        ("PATCH", "/api/v1/auth/me", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-007)"),
        ("GET", "/api/v1/users", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-008)"),
        ("PATCH", "/api/v1/users/{id}/status", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-008)"),
        ("GET", "/api/v1/auth/sessions", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-009)"),
        ("DELETE", "/api/v1/auth/sessions/{id}", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-009)"),
        ("POST", "/api/v1/auth/sessions/revoke-all", "Dev 1 — 2314236_HoangBinhQuan_buoiso4 (FR-AUTH-009)"),
        ("PUT", "/api/v1/recipes/{id}", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-004)"),
        ("POST", "/api/v1/recipes/{id}/ingredients", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-009)"),
        ("PUT", "/api/v1/recipes/{id}/ingredients/{ingId}", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-009)"),
        ("DELETE", "/api/v1/recipes/{id}/ingredients/{ingId}", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-009)"),
        ("POST", "/api/v1/recipes/{id}/steps", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-010)"),
        ("PUT", "/api/v1/recipes/{id}/steps/{stepId}", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-010)"),
        ("PATCH", "/api/v1/recipes/{id}/steps/reorder", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-010)"),
        ("DELETE", "/api/v1/recipes/{id}/steps/{stepId}", "Dev 2 — 2312758_NguyenHongPhucTho_buoiso4 (FR-RCP-010)"),
        ("GET", "/api/v1/recipes/mine", "Dev 3 — 2314291_DoanHongTien_buoiso4 (FR-RCP-011)"),
        ("GET", "/api/v1/recipes/mine/{id}", "Dev 3 — 2314291_DoanHongTien_buoiso4 (CR-2026-04 d)"),
        ("GET", "/api/v1/recipes/search", "Dev 3 — 2314291_DoanHongTien_buoiso4 (FR-SRCH-001)"),
        ("GET", "/api/v1/recipes/sitemap", "Dev 3 — 2314291_DoanHongTien_buoiso4 (MT-48)"),
    ];

    [Fact]
    public void SrsChapter8_Lists45Endpoints()
    {
        Assert.Equal(45, SrsEndpoints.Length);
        Assert.Equal(45, SrsEndpoints.Select(e => Key(e.Method, e.Route)).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EveryEndpointInSrs_IsImplemented()
    {
        var implemented = ImplementedEndpoints();
        var pending = PendingEndpoints.Select(e => Key(e.Method, e.Route)).ToHashSet(StringComparer.Ordinal);

        var missing = SrsEndpoints
            .Where(e => !implemented.Contains(Key(e.Method, e.Route)) && !pending.Contains(Key(e.Method, e.Route)))
            .Select(e => $"{e.Method} {e.Route} (SRS §{e.Srs})")
            .ToArray();

        Assert.True(missing.Length == 0, "Endpoint có trong SRS Chương 8 nhưng chưa cài đặt: " + string.Join("; ", missing));
    }

    [Fact]
    public void NoApiEndpoint_ExistsOutsideSrs()
    {
        var srs = SrsEndpoints.Select(e => Key(e.Method, e.Route)).ToHashSet(StringComparer.Ordinal);

        var extra = ImplementedEndpoints()
            .Where(k => k.Contains(" /api/v1", StringComparison.Ordinal) && !srs.Contains(k))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(extra.Length == 0, "Endpoint /api/v1 không có trong SRS Chương 8 (phải qua Change Request): " + string.Join("; ", extra));
    }

    [Fact]
    public void PendingEndpoints_AreReallyMissing()
    {
        var implemented = ImplementedEndpoints();

        var landed = PendingEndpoints
            .Where(e => implemented.Contains(Key(e.Method, e.Route)))
            .Select(e => $"{e.Method} {e.Route}")
            .ToArray();

        Assert.True(landed.Length == 0, "Endpoint đã được cài đặt — xóa khỏi PendingEndpoints: " + string.Join("; ", landed));
    }

    /// <summary>
    /// Endpoint không khai báo method (ví dụ MapHealthChecks) nhận MỌI method — tính là có GET, đúng như SRS §8.7 mô tả.
    /// </summary>
    private HashSet<string> ImplementedEndpoints() =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .SelectMany(e => (e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [HttpMethods.Get])
                .Select(method => Key(method, "/" + (e.RoutePattern.RawText ?? string.Empty).TrimStart('/'))))
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>"GET /api/v1/recipes/{id:guid}/" → "GET /api/v1/recipes/{}" — bỏ ràng buộc/tên tham số và dấu "/" cuối.</summary>
    private static string Key(string method, string route) =>
        $"{method.ToUpperInvariant()} {RouteParameter().Replace(route, "{}").TrimEnd('/')}";

    [GeneratedRegex(@"\{[^}]*\}")]
    private static partial Regex RouteParameter();
}
