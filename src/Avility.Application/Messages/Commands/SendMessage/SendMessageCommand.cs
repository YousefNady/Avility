using Avility.Application.Messages.Dtos;
using MediatR;

namespace Avility.Application.Messages.Commands.SendMessage;

public sealed record SendMessageCommand(Guid JobApplicationId, string Body) : IRequest<MessageDto>;