using MediatR;

namespace Avility.Application.Admin.Commands.SendTestEmail;

public sealed record SendTestEmailCommand(string ToEmail) : IRequest;