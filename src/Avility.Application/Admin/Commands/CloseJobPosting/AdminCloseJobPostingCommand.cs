using MediatR;

namespace Avility.Application.Admin.Commands.CloseJobPosting;

public sealed record AdminCloseJobPostingCommand(Guid JobPostingId) : IRequest;
