using Avility.Application.Common.Interfaces;
using Avility.Application.Messages;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Messages.Commands.MarkThreadAsRead;

public sealed class MarkThreadAsReadCommandHandler : IRequestHandler<MarkThreadAsReadCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IJobApplicationAccessGuard _accessGuard;
    private readonly IDateTime _dateTime;
    private readonly IMessageNotifier _messageNotifier;

    public MarkThreadAsReadCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser,
        IJobApplicationAccessGuard accessGuard,
        IDateTime dateTime,
        IMessageNotifier messageNotifier)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _accessGuard = accessGuard;
        _dateTime = dateTime;
        _messageNotifier = messageNotifier;
    }

    public async Task Handle(MarkThreadAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        await _accessGuard.EnsureParticipantAsync(request.JobApplicationId, userId, cancellationToken);

        // Only messages sent by the OTHER participant can be "unread" by
        // the caller - a sender's own messages are never marked unread
        // against themselves.
        var unreadMessages = await _dbContext.Messages
            .Where(m => m.JobApplicationId == request.JobApplicationId && m.SenderUserId != userId && !m.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadMessages.Count == 0)
        {
            return;
        }

        var readAt = _dateTime.UtcNow;
        foreach (var message in unreadMessages)
        {
            message.MarkAsRead(readAt);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _messageNotifier.NotifyThreadReadAsync(request.JobApplicationId, userId, cancellationToken);
    }
}