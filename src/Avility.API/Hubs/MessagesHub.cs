using System.IdentityModel.Tokens.Jwt;
using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Messages.Commands.SendMessage;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Avility.Application.Messages.Commands.MarkThreadAsRead;

namespace Avility.API.Hubs;

/// <summary>
/// Thin real-time adapter over the existing REST messaging flow - same
/// conceptual role as a Controller. No business logic lives here:
/// SendMessage delegates to the exact same SendMessageCommand the REST
/// endpoint uses, so validation, the participant check, and persistence
/// only exist in one place (SendMessageCommandHandler).
/// </summary>
[Authorize]
public sealed class MessagesHub : Hub
{
    private readonly ISender _sender;
    private readonly IJobApplicationAccessGuard _accessGuard;

    public MessagesHub(ISender sender, IJobApplicationAccessGuard accessGuard)
    {
        _sender = sender;
        _accessGuard = accessGuard;
    }

    public Task JoinThread(Guid jobApplicationId) => ExecuteAsync(async () =>
    {
        var userId = GetUserId();
        await _accessGuard.EnsureParticipantAsync(jobApplicationId, userId, Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(jobApplicationId));
    });

    public Task LeaveThread(Guid jobApplicationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(jobApplicationId));

    public Task SendMessage(Guid jobApplicationId, string body) => ExecuteAsync(() =>
        _sender.Send(new SendMessageCommand(jobApplicationId, body), Context.ConnectionAborted));
    
    public Task MarkThreadAsRead(Guid jobApplicationId) => ExecuteAsync(() =>
        _sender.Send(new MarkThreadAsReadCommand(jobApplicationId), Context.ConnectionAborted));

    /// <summary>
    /// SendMessageCommandHandler and the guard throw Application-layer
    /// exceptions that are meaningless to a raw client without
    /// translation (SignalR hides unhandled exception details by
    /// default). This is the Hub's equivalent of GlobalExceptionHandler -
    /// small enough here (one Hub, three methods) that a shared private
    /// helper is simpler than a HubFilter.
    /// </summary>
    private static async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (NotFoundException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (ForbiddenAccessException)
        {
            throw new HubException("You are not a participant in this conversation.");
        }
        catch (ValidationException ex)
        {
            throw new HubException(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
        }
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(value, out var id)
            ? id
            : throw new HubException("Unable to resolve the authenticated user.");
    }

    internal static string GroupName(Guid jobApplicationId) => $"job-application:{jobApplicationId}";
}