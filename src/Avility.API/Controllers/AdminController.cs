using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Admin.Commands.ActivateUser;
using Avility.Application.Admin.Commands.CloseJobPosting;
using Avility.Application.Admin.Commands.DeactivateUser;
using Avility.Application.Admin.Queries.GetPlatformStatistics;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.Companies.Dtos;
using Avility.Application.Companies.Queries.GetByVerificationStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avility.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminController : ControllerBase
{
    private readonly ISender _sender;

    public AdminController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("companies")]
    public async Task<ActionResult<ApiResponse<PagedResult<CompanyProfileDto>>>> GetCompanies(
        [FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetCompaniesByVerificationStatusQuery(status, pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<CompanyProfileDto>>.SuccessResponse(result));
    }
    
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<PlatformStatisticsDto>>> GetStatistics(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPlatformStatisticsQuery(), cancellationToken);
        return Ok(ApiResponse<PlatformStatisticsDto>.SuccessResponse(result));
    }

    [HttpPost("users/{userId:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(Guid userId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateUserCommand(userId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "User deactivated."));
    }

    [HttpPost("users/{userId:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(Guid userId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateUserCommand(userId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "User activated."));
    }

    [HttpPost("jobpostings/{jobPostingId:guid}/close")]
    public async Task<ActionResult<ApiResponse<object>>> CloseJobPosting(Guid jobPostingId, CancellationToken cancellationToken)
    {
        await _sender.Send(new AdminCloseJobPostingCommand(jobPostingId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Job posting closed."));
    }
}
