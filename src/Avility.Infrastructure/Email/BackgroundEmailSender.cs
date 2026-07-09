using Avility.Application.Common.Interfaces;

namespace Avility.Infrastructure.Email;

/// <summary>
/// Decorates SmtpEmailSender so callers get an immediate return instead of
/// waiting on a live SMTP round-trip. The actual send runs on
/// QueuedHostedService. Handlers/tests are unaffected: they only ever
/// depend on IEmailSender.
/// </summary>
public sealed class BackgroundEmailSender : IEmailSender
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly SmtpEmailSender _innerSender;

    public BackgroundEmailSender(IBackgroundTaskQueue queue, SmtpEmailSender innerSender)
    {
        _queue = queue;
        _innerSender = innerSender;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        _queue.QueueBackgroundWorkItem(ct => _innerSender.SendAsync(toEmail, subject, body, ct));
        return Task.CompletedTask;
    }
}