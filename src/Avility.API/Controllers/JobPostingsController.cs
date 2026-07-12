using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Commands.Close;
using Avility.Application.JobPostings.Commands.Create;
using Avility.Application.JobPostings.Commands.Publish;
using Avility.Application.JobPostings.Commands.Update;
using Avility.Application.JobPostings.Dtos;
using Avility.Application.JobPostings.Queries.GetById;
using Avility.Application.JobPostings.Queries.GetMine;
using Avility.Application.JobPostings.Queries.Search;
using Avility.Application.JobPostings.Queries.GetRecommended;
using Avility.Application.JobApplications.Dtos;
using Avility.Application.JobApplications.Queries.GetApplicants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avility.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/jobpostings")]
public sealed class JobPostingsController : ControllerBase
{
    private readonly ISender _sender;

    public JobPostingsController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<JobPostingDto>>>> Search([FromQuery] SearchJobPostingsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<JobPostingDto>>.SuccessResponse(result));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<PagedResult<JobPostingDto>>>> GetMine([FromQuery] GetMyJobPostingsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<JobPostingDto>>.SuccessResponse(result));
    }
    
    [Authorize(Roles = Roles.JobSeeker)]
    [HttpGet("recommended")]
    public async Task<ActionResult<ApiResponse<PagedResult<JobPostingDto>>>> GetRecommended([FromQuery] GetRecommendedJobPostingsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<JobPostingDto>>.SuccessResponse(result));
    }

    [AllowAnonymous]
    [HttpGet("{jobPostingId:guid}")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> GetById(Guid jobPostingId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetJobPostingByIdQuery(jobPostingId), cancellationToken);
        return Ok(ApiResponse<JobPostingDto>.SuccessResponse(result));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> Create(CreateJobPostingCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<JobPostingDto>.SuccessResponse(result, "Job posting created."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPut("{jobPostingId:guid}")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> Update(Guid jobPostingId, UpdateJobPostingRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateJobPostingCommand(
            jobPostingId, request.Title, request.Description, request.Requirements,
            request.EmploymentType, request.ExperienceLevel, request.IsRemote,
            request.Country, request.Governorate, request.City,
            request.SalaryMin, request.SalaryMax, request.SalaryCurrency, request.ApplicationDeadline,
            request.SupportedDisabilityCategories, request.AccommodationDetails);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<JobPostingDto>.SuccessResponse(result, "Job posting updated."));
    }
    
    [Authorize(Roles = Roles.Company)]
    [HttpGet("{jobPostingId:guid}/applicants")]
    public async Task<ActionResult<ApiResponse<PagedResult<JobApplicationDto>>>> GetApplicants(
        Guid jobPostingId, [FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetApplicantsForJobPostingQuery(jobPostingId, status, pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<JobApplicationDto>>.SuccessResponse(result));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("{jobPostingId:guid}/publish")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> Publish(Guid jobPostingId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PublishJobPostingCommand(jobPostingId), cancellationToken);
        return Ok(ApiResponse<JobPostingDto>.SuccessResponse(result, "Job posting published."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("{jobPostingId:guid}/close")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> Close(Guid jobPostingId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CloseJobPostingCommand(jobPostingId), cancellationToken);
        return Ok(ApiResponse<JobPostingDto>.SuccessResponse(result, "Job posting closed."));
    }
}

public sealed record UpdateJobPostingRequest(
    string Title,
    string Description,
    string? Requirements,
    string EmploymentType,
    string ExperienceLevel,
    bool IsRemote,
    string? Country,
    string? Governorate,
    string? City,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    DateTime? ApplicationDeadline,
    IReadOnlyList<string>? SupportedDisabilityCategories = null,
    string? AccommodationDetails = null);
