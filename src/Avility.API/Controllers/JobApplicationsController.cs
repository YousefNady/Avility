using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.JobApplications.Commands.Accept;
using Avility.Application.JobApplications.Commands.Apply;
using Avility.Application.JobApplications.Commands.MoveToUnderReview;
using Avility.Application.JobApplications.Commands.Reject;
using Avility.Application.JobApplications.Commands.Withdraw;
using Avility.Application.JobApplications.Dtos;
using Avility.Application.JobApplications.Queries.GetMine;
using Avility.Application.Messages.Commands.SendMessage;
using Avility.Application.Messages.Dtos;
using Avility.Application.Messages.Queries.GetThread;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avility.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/jobapplications")]
public sealed class JobApplicationsController : ControllerBase
{
    private readonly ISender _sender;

    public JobApplicationsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = Roles.JobSeeker)]
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<PagedResult<JobApplicationDto>>>> GetMine([FromQuery] GetMyJobApplicationsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<JobApplicationDto>>.SuccessResponse(result));
    }

    [Authorize(Roles = Roles.JobSeeker)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> Apply(ApplyToJobCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<JobApplicationDto>.SuccessResponse(result, "Application submitted."));
    }

    [Authorize(Roles = Roles.JobSeeker)]
    [HttpPost("{jobApplicationId:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> Withdraw(Guid jobApplicationId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new WithdrawJobApplicationCommand(jobApplicationId), cancellationToken);
        return Ok(ApiResponse<JobApplicationDto>.SuccessResponse(result, "Application withdrawn."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("{jobApplicationId:guid}/under-review")]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> MoveToUnderReview(Guid jobApplicationId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MoveApplicationToUnderReviewCommand(jobApplicationId), cancellationToken);
        return Ok(ApiResponse<JobApplicationDto>.SuccessResponse(result, "Application moved to under review."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("{jobApplicationId:guid}/accept")]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> Accept(Guid jobApplicationId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AcceptJobApplicationCommand(jobApplicationId), cancellationToken);
        return Ok(ApiResponse<JobApplicationDto>.SuccessResponse(result, "Application accepted."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("{jobApplicationId:guid}/reject")]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> Reject(Guid jobApplicationId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectJobApplicationCommand(jobApplicationId), cancellationToken);
        return Ok(ApiResponse<JobApplicationDto>.SuccessResponse(result, "Application rejected."));
    }
    
    [Authorize]
    [HttpPost("{jobApplicationId:guid}/messages")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage(Guid jobApplicationId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SendMessageCommand(jobApplicationId, request.Body), cancellationToken);
        return Ok(ApiResponse<MessageDto>.SuccessResponse(result, "Message sent."));
    }

    [Authorize]
    [HttpGet("{jobApplicationId:guid}/messages")]
    public async Task<ActionResult<ApiResponse<PagedResult<MessageDto>>>> GetThread(
        Guid jobApplicationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetThreadQuery(jobApplicationId, pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<MessageDto>>.SuccessResponse(result));
    }
}

public sealed record SendMessageRequest(string Body);
