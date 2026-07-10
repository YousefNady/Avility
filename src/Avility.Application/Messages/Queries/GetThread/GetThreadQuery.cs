using Avility.Application.Common.Models;
using Avility.Application.Messages.Dtos;
using MediatR;

namespace Avility.Application.Messages.Queries.GetThread;

public sealed record GetThreadQuery(
    Guid JobApplicationId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<MessageDto>>;