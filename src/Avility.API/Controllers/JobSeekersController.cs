using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.JobSeekers.Commands.CreateProfile;
using Avility.Application.JobSeekers.Commands.UpdateProfile;
using Avility.Application.JobSeekers.Dtos;
using Avility.Application.JobSeekers.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avility.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/jobseekers")]
[Authorize(Roles = Roles.JobSeeker)]
public sealed class JobSeekersController : ControllerBase
{
    private readonly ISender _sender;

    public JobSeekersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<JobSeekerProfileDto>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyJobSeekerProfileQuery(), cancellationToken);
        return Ok(ApiResponse<JobSeekerProfileDto>.SuccessResponse(result));
    }

    [HttpPost("me")]
    public async Task<ActionResult<ApiResponse<JobSeekerProfileDto>>> CreateMyProfile(CreateJobSeekerProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<JobSeekerProfileDto>.SuccessResponse(result, "Profile created."));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<JobSeekerProfileDto>>> UpdateMyProfile(UpdateJobSeekerProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<JobSeekerProfileDto>.SuccessResponse(result, "Profile updated."));
    }
}
