using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.JobSeekers.Commands.CreateProfile;
using Avility.Application.JobSeekers.Commands.UpdateProfile;
using Avility.Application.JobSeekers.Commands.UploadResume;
using Avility.Application.JobSeekers.Queries.GetMyResume;
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
    
    [HttpPost("me/resume")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<JobSeekerProfileDto>>> UploadResume(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("A resume file is required."));
        }

        await using var stream = file.OpenReadStream();
        var command = new UploadJobSeekerResumeCommand(stream, file.FileName, file.ContentType, file.Length);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<JobSeekerProfileDto>.SuccessResponse(result, "Resume uploaded."));
    }

    [HttpGet("me/resume")]
    public async Task<IActionResult> DownloadResume(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyResumeQuery(), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
