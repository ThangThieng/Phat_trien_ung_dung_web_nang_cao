using System.Globalization;
using System.Threading.RateLimiting;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.HttpOverrides;

namespace CulinaryBlog.API.Extensions;

/// <summary>IP quotas are evaluated only after trusted forwarded headers have been processed.</summary>
public sealed class SecurityRateLimits : IDisposable
{
    public const string Auth = "auth";
    public const string Upload = "upload";
    private readonly SecurityRateLimitingOptions _options;

    public SecurityRateLimits(Microsoft.Extensions.Options.IOptions<SecurityRateLimitingOptions> options)
    {
        _options = options.Value;
        var global = CreateLimiter("general", _options.General);
        var auth = CreateLimiter(Auth, _options.Auth);
        var upload = CreateLimiter(Upload, _options.Upload);
        Limiter = PartitionedRateLimiter.CreateChained(global, auth, upload);
    }

    public PartitionedRateLimiter<HttpContext> Limiter { get; }

    public (string Name, int Limit, TimeSpan Window) GetPolicy(HttpContext context)
    {
        if (!_options.Enabled || IsExempt(context))
        {
            return ("none", 0, TimeSpan.Zero);
        }

        var name = context.GetEndpoint()?.Metadata.GetMetadata<SecurityRateLimitMetadata>()?.Name;
        var quota = name switch
        {
            Auth => _options.Auth,
            Upload => _options.Upload,
            _ => _options.General,
        };
        if (quota.PermitLimit > _options.General.PermitLimit)
        {
            name = "general";
            quota = _options.General;
        }

        return (name ?? "general", quota.PermitLimit, TimeSpan.FromSeconds(quota.WindowSeconds));
    }

    public static bool IsExempt(HttpContext context) =>
        HttpMethods.IsOptions(context.Request.Method) ||
        (context.Request.Path.Value?.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ?? false) ||
        context.Request.Path.StartsWithSegments("/hangfire");

    public void Dispose() => Limiter.Dispose();

    private PartitionedRateLimiter<HttpContext> CreateLimiter(string name, RateQuota quota) =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var selected = context.GetEndpoint()?.Metadata.GetMetadata<SecurityRateLimitMetadata>()?.Name;
            if (!_options.Enabled || IsExempt(context) || (name != "general" && selected != name))
            {
                return RateLimitPartition.GetNoLimiter("exempt");
            }

            var key = $"{name}:{context.Connection.RemoteIpAddress?.MapToIPv6()}";
            return name == Auth
                ? RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = quota.PermitLimit,
                    Window = TimeSpan.FromSeconds(quota.WindowSeconds),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0,
                    AutoReplenishment = true,
                })
                : RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = quota.PermitLimit,
                    Window = TimeSpan.FromSeconds(quota.WindowSeconds),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
        });
}

public sealed record SecurityRateLimitMetadata(string Name);

public static class SecurityExtensions
{
    public const string CorsPolicy = "ConfiguredOrigins";

    public static IServiceCollection AddSecurityCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Any(origin => origin.Contains('*') || !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            uri.UserInfo.Length > 0 || origin != uri.GetLeftPart(UriPartial.Authority)))
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain explicit HTTP(S) origins without paths or wildcards.");
        }

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID", "If-Match")
            .WithExposedHeaders("X-Correlation-ID", "ETag", "Retry-After", "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")));
        return services;
    }

    public static IServiceCollection AddTrustedForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            foreach (var cidr in configuration.GetSection("ForwardedHeaders:TrustedNetworks").Get<string[]>() ?? ["172.16.0.0/12"])
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(cidr));
            }
        });
        return services;
    }

    public static IApplicationBuilder UseRequestSecurity(this IApplicationBuilder app)
    {
        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseSecurityHeaders();
        app.UseStatusCodePages();
        app.UseCors(CorsPolicy);
        app.UseRateLimitHeaders();
        app.UseRateLimiter();
        app.UseUnsupportedVersionContract();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static IServiceCollection AddSecurityRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SecurityRateLimitingOptions>().Bind(configuration.GetSection("RateLimiting"))
            .Validate(
                options => new[] { options.Auth, options.General, options.Upload }.All(quota => quota.PermitLimit > 0 && quota.WindowSeconds > 0),
                "Rate quotas require positive PermitLimit and WindowSeconds.")
            .ValidateOnStart();
        services.AddSingleton<SecurityRateLimits>();
        services.AddRateLimiter(_ => { });
        services.AddOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>()
            .Configure<SecurityRateLimits>((options, limits) =>
            {
                options.GlobalLimiter = limits.Limiter;
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (rejected, ct) =>
                {
                    var retry = rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? retryAfter : limits.GetPolicy(rejected.HttpContext).Window;
                    var seconds = Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds));
                    rejected.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                    rejected.HttpContext.Response.Headers["X-RateLimit-Reset"] =
                        DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
                    await Results.Problem(
                        statusCode: StatusCodes.Status429TooManyRequests,
                        type: ErrorCodes.RateLimitExceeded,
                        title: "Too Many Requests",
                        detail: "Quá nhiều yêu cầu. Vui lòng thử lại sau.")
                        .ExecuteAsync(rejected.HttpContext).ConfigureAwait(false);
                };
            });
        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // API responses contain no executable scripts; Scalar has its own development UI policy.
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers.ContentSecurityPolicy =
                "default-src 'none'; script-src 'none'; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
        }

        await next(context).ConfigureAwait(false);
    });

    public static IApplicationBuilder UseRateLimitHeaders(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var limits = context.RequestServices.GetRequiredService<SecurityRateLimits>();
        var policy = limits.GetPolicy(context);
        if (policy.Limit > 0)
        {
            context.Response.OnStarting(() =>
            {
                var remaining = limits.Limiter.GetStatistics(context)?.CurrentAvailablePermits ?? 0;
                context.Response.Headers["X-RateLimit-Limit"] = policy.Limit.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString(CultureInfo.InvariantCulture);

                // Conservative full-replenishment horizon; rejected requests use the lease's RetryAfter instead.
                if (!context.Response.Headers.ContainsKey("X-RateLimit-Reset"))
                {
                    context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(policy.Window)
                        .ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
                }

                return Task.CompletedTask;
            });
        }

        await next(context).ConfigureAwait(false);
    });
}
