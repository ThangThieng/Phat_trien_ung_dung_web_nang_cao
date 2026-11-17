using System.Diagnostics.Metrics;

namespace CulinaryBlog.Application.Common.Diagnostics;

/// <summary>
/// FR-OBS-003 (mức C — Buổi 8): metric nghiệp vụ "recipe created/published count". Dùng <see cref="Meter"/> của BCL nên tầng
/// Application không phụ thuộc OpenTelemetry; tầng API đăng ký <see cref="MeterName"/> với OpenTelemetry để xuất qua OTLP. Không có
/// listener nào thì <c>Add</c> gần như không tốn gì.
/// </summary>
public static class CulinaryBlogMetrics
{
    public const string MeterName = "CulinaryBlog";

    /// <summary>Khai báo trước các counter: field static khởi tạo theo thứ tự xuất hiện.</summary>
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    /// <summary>Công thức (bản nháp) được tạo — <c>POST /recipes</c>.</summary>
    public static readonly Counter<long> RecipesCreated =
        Meter.CreateCounter<long>("recipes_created_total", unit: "{recipe}", description: "Số công thức được tạo (bản nháp).");

    /// <summary>Lượt xuất bản thành công — <c>PATCH /recipes/{id}/publish</c>.</summary>
    public static readonly Counter<long> RecipesPublished =
        Meter.CreateCounter<long>("recipes_published_total", unit: "{recipe}", description: "Số lượt xuất bản công thức thành công.");
}
