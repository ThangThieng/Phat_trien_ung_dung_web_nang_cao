using System.Security.Cryptography;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace CulinaryBlog.API.Extensions;

/// <summary>
/// FR-JOB-001 / MT-39: Hangfire Dashboard là trang HTML mở thẳng trong trình duyệt nên KHÔNG thể dùng
/// JWT Bearer của ứng dụng (SRS §5.2 cũng loại bỏ cookie). Lớp bảo vệ chính là HTTP Basic Auth ở Nginx;
/// filter này là lớp thứ hai: chỉ cho qua request mang header <c>X-Hangfire-Gate</c> khớp secret mà
/// Nginx gắn vào SAU khi người dùng đã qua Basic Auth. Nhờ vậy, gọi thẳng <c>:5000/hangfire</c>
/// (cổng dev của container api, bỏ qua Nginx) vẫn bị chặn.
/// </summary>
/// <remarks>
/// KHÔNG dùng <c>LocalRequestsOnlyAuthorizationFilter</c> mặc định của Hangfire: request đi qua reverse proxy
/// mang IP của container Nginx (không phải loopback) nên filter đó từ chối mọi truy cập hợp lệ.
/// </remarks>
public sealed class NginxGateDashboardFilter(string gateSecret) : IDashboardAuthorizationFilter
{
    /// <summary>Header do Nginx gắn vào (không phải người dùng gửi) – xem nginx/templates/nginx.conf.template.</summary>
    public const string HeaderName = "X-Hangfire-Gate";

    private readonly byte[] _expected = Encoding.UTF8.GetBytes(gateSecret);

    public bool Authorize([NotNull] DashboardContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Secret rỗng = cấu hình sai → đóng cửa thay vì mở toang (fail closed).
        if (_expected.Length == 0)
        {
            return false;
        }

        var presented = context.GetHttpContext().Request.Headers[HeaderName].ToString();
        if (presented.Length == 0)
        {
            return false;
        }

        // FixedTimeEquals: so sánh không phụ thuộc độ dài trùng khớp đầu chuỗi (chống timing attack).
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(presented), _expected);
    }
}
