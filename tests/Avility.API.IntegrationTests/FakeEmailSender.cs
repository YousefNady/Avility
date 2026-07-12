using System.Collections.Concurrent;
using Avility.Application.Common.Interfaces;

namespace Avility.API.IntegrationTests;

/// <summary>
/// Test double replacing SmtpEmailSender so integration tests never touch
/// a real mail server. Captures every "sent" message so tests can extract
/// values (e.g. a password reset token) that only exist in the email body.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    public sealed record SentEmail(string ToEmail, string Subject, string Body);

    private static readonly ConcurrentBag<SentEmail> _sent = new();

    public static IReadOnlyCollection<SentEmail> Sent => _sent.ToList();

    public static void Clear() => _sent.Clear();

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        _sent.Add(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }
}