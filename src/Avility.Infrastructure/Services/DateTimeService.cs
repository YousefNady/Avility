using Avility.Application.Common.Interfaces;

namespace Avility.Infrastructure.Services;

public sealed class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
