namespace Avility.Domain.Exceptions;

/// <summary>
/// Base type for every exception raised by a domain invariant violation.
/// Kept separate from ordinary .NET exceptions so the Application layer
/// can catch DomainException specifically (e.g. in a MediatR pipeline
/// behavior or API exception-handling middleware) and translate it into a
/// 400-level API response, distinct from an unexpected 500 error.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
