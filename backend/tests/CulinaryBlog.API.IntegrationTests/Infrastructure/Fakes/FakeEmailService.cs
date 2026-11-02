using CulinaryBlog.Infrastructure.Email;

namespace CulinaryBlog.API.IntegrationTests.Infrastructure.Fakes;

public sealed class FakeEmailService : IEmailService
{
    private readonly List<SentEmail> _sent = [];

    public IReadOnlyList<SentEmail> Sent => _sent;

    public Task SendAsync(string toAddress, string toName, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        _sent.Add(new SentEmail(toAddress, toName, subject, htmlBody));
        return Task.CompletedTask;
    }

    public void Clear() => _sent.Clear();

    public sealed record SentEmail(string ToAddress, string ToName, string Subject, string HtmlBody);
}
