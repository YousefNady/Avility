namespace Avility.Application.Common.Interfaces;

/// <summary>
/// Abstraction over outbound email delivery. Infrastructure owns the
/// concrete transport (SMTP today; swappable for a provider-backed sender
/// later without touching any handler).
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
}