using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.Companies.Commands.CreateProfile;
using Avility.Application.Companies.Commands.RejectCompany;
using Avility.Application.Companies.Commands.UpdateProfile;
using Avility.Application.Companies.Commands.VerifyCompany;
using Avility.Application.Companies.Dtos;
using Avility.Application.Companies.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avility.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ISender _sender;

    public CompaniesController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = Roles.Company)]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyCompanyProfileQuery(), cancellationToken);
        return Ok(ApiResponse<CompanyProfileDto>.SuccessResponse(result));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("me")]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto>>> CreateMyProfile(CreateCompanyProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<CompanyProfileDto>.SuccessResponse(result, "Profile created."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto>>> UpdateMyProfile(UpdateCompanyProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<CompanyProfileDto>.SuccessResponse(result, "Profile updated."));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{companyId:guid}/verify")]
    public async Task<ActionResult<ApiResponse<object>>> Verify(Guid companyId, CancellationToken cancellationToken)
    {
        await _sender.Send(new VerifyCompanyCommand(companyId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Company verified."));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{companyId:guid}/reject")]
    public async Task<ActionResult<ApiResponse<object>>> Reject(Guid companyId, CancellationToken cancellationToken)
    {
        await _sender.Send(new RejectCompanyCommand(companyId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Company rejected."));
    }
}
