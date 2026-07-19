using Avility.Application.Messages;
using Avility.Application.Messages.Dtos;
using Microsoft.AspNetCore.SignalR;


namespace Avility.API.Hubs;

/// <summary>
/// Lives in API (not Infrastructure) because it needs the concrete
/// MessagesHub type via IHubContext&lt;MessagesHub&gt; - Infrastructure
/// can't depend on API without breaking the inward-only dependency
/// direction. Broadcasts only to the thread's own group, never
/// Clients.All.
/// </summary>
public sealed class SignalRMessageNotifier : IMessageNotifier
{
    private readonly IHubContext<MessagesHub> _hubContext;

    public SignalRMessageNotifier(IHubContext<MessagesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyAsync(MessageDto message, CancellationToken cancellationToken) =>
        _hubContext.Clients
            .Group(MessagesHub.GroupName(message.JobApplicationId))
            .SendAsync("MessageReceived", message, cancellationToken);
    
    public Task NotifyThreadReadAsync(Guid jobApplicationId, Guid readByUserId, CancellationToken cancellationToken) =>
        _hubContext.Clients
            .Group(MessagesHub.GroupName(jobApplicationId))
            .SendAsync("MessagesRead", new { jobApplicationId, readByUserId }, cancellationToken);
}