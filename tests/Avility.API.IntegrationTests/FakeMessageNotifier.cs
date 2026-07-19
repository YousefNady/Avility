using System.Collections.Concurrent;
using Avility.Application.Messages;
using Avility.Application.Messages.Dtos;

namespace Avility.API.IntegrationTests;

/// <summary>
/// Test double replacing SignalRMessageNotifier so integration tests can
/// assert a broadcast was attempted without needing a real Hub
/// connection. Proves the SendMessageCommandHandler -> IMessageNotifier
/// wiring; a full socket round-trip test would need the
/// Microsoft.AspNetCore.SignalR.Client package, which isn't added here -
/// flagging that as an optional follow-up rather than adding a new test
/// dependency unasked.
/// </summary>
public sealed class FakeMessageNotifier : IMessageNotifier
{
    private static readonly ConcurrentBag<MessageDto> _notified = new();

    public static IReadOnlyCollection<MessageDto> Notified => _notified.ToList();

    public static void Clear() => _notified.Clear();

    public Task NotifyAsync(MessageDto message, CancellationToken cancellationToken)
    {
        _notified.Add(message);
        return Task.CompletedTask;
    }
    
    public sealed record ReadReceipt(Guid JobApplicationId, Guid ReadByUserId);

    private static readonly ConcurrentBag<ReadReceipt> _readReceipts = new();

    public static IReadOnlyCollection<ReadReceipt> ReadReceipts => _readReceipts.ToList();

    public Task NotifyThreadReadAsync(Guid jobApplicationId, Guid readByUserId, CancellationToken cancellationToken)
    {
        _readReceipts.Add(new ReadReceipt(jobApplicationId, readByUserId));
        return Task.CompletedTask;
    }
}