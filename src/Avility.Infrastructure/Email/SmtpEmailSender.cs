using System.Net;
using System.Net.Mail;
using Avility.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avility.Infrastructure.Email;

/// <summary>
/// Built-in System.Net.Mail transport - no paid provider or extra NuGet
/// package required. Works against any SMTP server (a local dev catcher,
/// a free provider, or a production relay later) purely through
/// configuration. If Smtp:Host isn't configured, sending is skipped with
/// a warning rather than throwing, so the app stays runnable out of the
/// box in dev without any external service.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("Smtp:Host is not configured - skipping email to {ToEmail}.", toEmail);
            return;
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage(
            new MailAddress(_settings.FromAddress, _settings.FromName),
            new MailAddress(toEmail))
        {
            Subject = subject,
            Body = body
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}