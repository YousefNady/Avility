namespace Avility.Application.Common.Interfaces;

/// <summary>
/// Lets handlers know who's calling (e.g. "create a JobSeeker profile for
/// the current user") without depending on HttpContext directly.
/// Interface only for now - the concrete implementation reads JWT claims
/// via IHttpContextAccessor, which only makes sense once the Auth
/// milestone exists. Declaring the abstraction now means handlers built
/// later can take a dependency on it immediately.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
