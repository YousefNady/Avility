namespace Avility.Application.Common.Interfaces;

/// <summary>
/// A minimal, in-process background work queue. Backed by an in-memory
/// channel in Infrastructure - no external broker, no paid service, no new
/// dependency. Sufficient for fire-and-forget work (e.g. sending an email)
/// that shouldn't block the HTTP request that triggered it.
/// </summary>
public interface IBackgroundTaskQueue
{
    void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
    Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}