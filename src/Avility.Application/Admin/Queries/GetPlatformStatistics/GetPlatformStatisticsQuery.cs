using MediatR;

namespace Avility.Application.Admin.Queries.GetPlatformStatistics;

public sealed record GetPlatformStatisticsQuery : IRequest<PlatformStatisticsDto>;