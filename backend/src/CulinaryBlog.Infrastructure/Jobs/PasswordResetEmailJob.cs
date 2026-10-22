using System.Net;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Infrastructure.Email;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>FR-AUTH-011. The reset token is generated here, never carried by Hangfire arguments or logs.</summary>
public sealed partial class PasswordResetEmailJob(
    IIdentityService identityService,
    IEmailService emailService,
    IOptions<SmtpOptions> smtpOptions,
    ILogger<PasswordResetEmailJob> logger)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 1800])]
    public async Task ExecuteAsync(string userId, string correlationId, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var token = await identityService.GeneratePasswordResetTokenAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null || string.IsNullOrWhiteSpace(token))
        {
            LogSkipped(logger, userId, correlationId);
            return;
        }

        var siteUrl = smtpOptions.Value.SiteUrl.TrimEnd('/');
        var link = $"{siteUrl}/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
        var name = WebUtility.HtmlEncode(user.DisplayName);
        var html = $"<p>Chào {name},</p><p>Đặt lại mật khẩu của bạn trong vòng 30 phút: <a href=\"{WebUtility.HtmlEncode(link)}\">Đặt lại mật khẩu</a>.</p>";
        await emailService.SendAsync(user.Email, user.DisplayName, "Đặt lại mật khẩu Culinary Blog", html, cancellationToken).ConfigureAwait(false);
        LogSent(logger, userId, correlationId);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Password reset email skipped for user {UserId}, correlation {CorrelationId}")]
    private static partial void LogSkipped(ILogger logger, string userId, string correlationId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset email sent for user {UserId}, correlation {CorrelationId}")]
    private static partial void LogSent(ILogger logger, string userId, string correlationId);
}

public sealed class HangfirePasswordResetEmailScheduler(IBackgroundJobClient jobs) : IPasswordResetEmailScheduler
{
    public void SchedulePasswordResetEmail(string userId, string correlationId) =>
        jobs.Enqueue<PasswordResetEmailJob>(job => job.ExecuteAsync(userId, correlationId, CancellationToken.None));
}
