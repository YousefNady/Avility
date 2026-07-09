using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.Companies.Commands.CreateProfile;
using Avility.Application.Companies.Commands.RejectCompany;
using Avility.Application.Companies.Commands.UpdateProfile;
using Avility.Application.Companies.Commands.VerifyCompany;
using Avility.Application.Companies.Dtos;
using Avility.Application.Companies.Queries.GetMyProfile;
using Avility.Application.Companies.Commands.DeleteLogo;
using Avility.Application.Companies.Commands.UploadLogo;
using Avility.Application.Companies.Queries.GetCompanyLogo;
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
    
    [Authorize(Roles = Roles.Company)]
    [HttpPost("me/logo")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto>>> UploadLogo(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("A logo file is required."));
        }

        await using var stream = file.OpenReadStream();
        var command = new UploadCompanyLogoCommand(stream, file.FileName, file.ContentType, file.Length);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<CompanyProfileDto>.SuccessResponse(result, "Logo uploaded."));
    }

    [Authorize(Roles = Roles.Company)]
    [HttpDelete("me/logo")]
    public async Task<ActionResult<ApiResponse<CompanyProfileDto>>> DeleteLogo(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCompanyLogoCommand(), cancellationToken);
        return Ok(ApiResponse<CompanyProfileDto>.SuccessResponse(result, "Logo removed."));
    }

    [AllowAnonymous]
    [HttpGet("{companyId:guid}/logo")]
    public async Task<IActionResult> GetLogo(Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCompanyLogoQuery(companyId), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
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
