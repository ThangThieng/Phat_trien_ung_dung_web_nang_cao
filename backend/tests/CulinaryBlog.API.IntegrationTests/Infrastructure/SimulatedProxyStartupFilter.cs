using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure;

/// <summary>
/// TestServer không có kết nối TCP nên <c>RemoteIpAddress</c> luôn null. Filter này gán địa chỉ của "container Nginx" trong
/// dải mạng Docker (172.16.0.0/12 — cấu hình ForwardedHeaders:TrustedNetworks) để request trong test đi đúng đường của
/// production: Nginx → API. Nhờ vậy <c>UseForwardedHeaders</c> (Buổi 4) được kiểm chứng thật với header X-Forwarded-For.
/// </summary>
public sealed class SimulatedProxyStartupFilter : IStartupFilter
{
    public static readonly IPAddress NginxAddress = IPAddress.Parse("172.18.0.5");

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress ??= NginxAddress;
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
