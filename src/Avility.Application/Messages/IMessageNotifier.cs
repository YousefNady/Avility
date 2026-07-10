using Avility.Application.Messages.Dtos;

namespace Avility.Application.Messages;

/// <summary>
/// Pushes a just-sent message to whoever is live-connected to its thread.
/// Application has no idea this is SignalR under the hood - same shape as
/// IEmailSender. A notifier with nobody connected is a no-op, not an
/// error. Lives alongside the Messages feature (not Common/Interfaces)
/// since, unlike IEmailSender/IDateTime/ICurrentUserService, this is
/// feature-specific rather than a cross-cutting concern.
/// </summary>
public interface IMessageNotifier
{
    Task NotifyAsync(MessageDto message, CancellationToken cancellationToken);
}