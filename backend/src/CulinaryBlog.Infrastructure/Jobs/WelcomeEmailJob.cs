using System.Net;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Email;
using CulinaryBlog.Infrastructure.Identity;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// FR-JOB-001 – Welcome Email (fire-and-forget). Retry 3 lần với backoff 1 phút, 5 phút, 30 phút;
/// sau 3 lần thất bại Hangfire chuyển job sang Failed state.
/// </summary>
public sealed partial class WelcomeEmailJob(
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IOptions<SmtpOptions> smtpOptions,
    ILogger<WelcomeEmailJob> logger)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 1800], OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user?.Email is null)
        {
            LogUserMissing(logger, userId);
            return;
        }

        var name = WebUtility.HtmlEncode(user.DisplayName);
        var siteUrl = smtpOptions.Value.SiteUrl.TrimEnd('/');
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:560px;margin:auto">
              <h2 style="color:#c2410c">Chào mừng {name} đến với Culinary Blog! 🍜</h2>
              <p>Tài khoản tác giả của bạn đã được tạo thành công. Giờ bạn có thể chia sẻ công thức nấu ăn của mình với cộng đồng.</p>
              <p><a href="{siteUrl}/recipes" style="background:#ea580c;color:#fff;padding:10px 18px;border-radius:6px;text-decoration:none">Khám phá công thức</a></p>
              <p style="color:#6b7280;font-size:12px">Bạn nhận được email này vì đã đăng ký tại {siteUrl}.</p>
            </div>
            """;

        await emailService.SendAsync(user.Email, user.DisplayName, "Chào mừng bạn đến với Culinary Blog", html, cancellationToken).ConfigureAwait(false);
        LogSent(logger, userId);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Welcome email skipped: user {UserId} not found")]
    private static partial void LogUserMissing(ILogger logger, string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Welcome email sent to user {UserId}")]
    private static partial void LogSent(ILogger logger, string userId);
}

/// <summary>IWelcomeEmailScheduler → BackgroundJob.Enqueue (FR-AUTH-001 bước 11).</summary>
public sealed class HangfireWelcomeEmailScheduler(IBackgroundJobClient jobs) : IWelcomeEmailScheduler
{
    public void ScheduleWelcomeEmail(string userId) =>
        jobs.Enqueue<WelcomeEmailJob>(job => job.ExecuteAsync(userId, CancellationToken.None));
}
