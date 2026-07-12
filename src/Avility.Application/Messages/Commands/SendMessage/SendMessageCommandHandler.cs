using Avility.Application.Common.Interfaces;
using Avility.Application.Messages.Dtos;
using Avility.Domain.Entities;
using MediatR;

namespace Avility.Application.Messages.Commands.SendMessage;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IJobApplicationAccessGuard _accessGuard;
    private readonly IMessageNotifier _messageNotifier;

    public SendMessageCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser,
        IJobApplicationAccessGuard accessGuard,
        IMessageNotifier messageNotifier)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _accessGuard = accessGuard;
        _messageNotifier = messageNotifier;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        await _accessGuard.EnsureParticipantAsync(request.JobApplicationId, userId, cancellationToken);

        var message = Message.Create(request.JobApplicationId, userId, request.Body);

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = message.ToDto();
        await _messageNotifier.NotifyAsync(dto, cancellationToken);

        return dto;
    }
}