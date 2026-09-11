using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CulinaryBlog.Infrastructure.Email;

/// <summary>SRS §5.3 – Smtp__Host, Smtp__Port, Smtp__Username, Smtp__Password. Development: Mailhog.</summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool UseSsl { get; set; } = true;

    public string FromAddress { get; set; } = "no-reply@culinaryblog.com";

    public string FromName { get; set; } = "Culinary Blog";

    /// <summary>Link ứng dụng chèn vào email.</summary>
    public string SiteUrl { get; set; } = "http://localhost:3000";
}

public interface IEmailService
{
    Task SendAsync(string toAddress, string toName, string subject, string htmlBody, CancellationToken cancellationToken);
}

/// <summary>IEmailService qua MailKit SMTP (TLS khi UseSsl = true).</summary>
public sealed class MailKitEmailService(IOptions<SmtpOptions> options) : IEmailService
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string toAddress, string toName, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var security = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, security, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, cancellationToken).ConfigureAwait(false);
        }

        await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);
    }
}
