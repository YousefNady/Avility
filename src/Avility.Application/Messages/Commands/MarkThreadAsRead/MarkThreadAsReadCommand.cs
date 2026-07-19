using MediatR;

namespace Avility.Application.Messages.Commands.MarkThreadAsRead;

public sealed record MarkThreadAsReadCommand(Guid JobApplicationId) : IRequest;