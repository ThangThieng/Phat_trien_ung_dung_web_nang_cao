namespace CulinaryBlog.API.Extensions;

public sealed class SecurityRateLimitingOptions
{
    public bool Enabled { get; set; } = true;

    public RateQuota Auth { get; set; } = new() { PermitLimit = 10 };

    public RateQuota General { get; set; } = new() { PermitLimit = 100 };

    public RateQuota Upload { get; set; } = new() { PermitLimit = 5 };

    /// <summary>
    /// CR-2026-10 (SRS v1.2.8, MT-73): hạn mức CHUNG cho các lần Frontend render trên server gọi API để nạp Data Cache dùng
    /// chung. Các request này cố ý không mang IP người xem (khóa cache phải ổn định), nên trước đây cả hệ thống chia nhau hạn mức
    /// 100/phút của IP container Frontend — một bộ thu thập đọc 100 trang khác nhau là mọi người khác nhận lỗi 500.
    /// </summary>
    public RateQuota Render { get; set; } = new() { PermitLimit = 1000 };

    /// <summary>
    /// Bí mật dùng chung giữa Frontend và API (biến môi trường <c>RENDER_GATE_SECRET</c>). Rỗng → không request nào được nhận là
    /// request render, mọi request theo hạn mức IP như NFR-SEC-003. Nginx xóa header này ở mọi request từ bên ngoài.
    /// </summary>
    public string RenderGateSecret { get; set; } = string.Empty;
}

public sealed class RateQuota
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; } = 60;
}
