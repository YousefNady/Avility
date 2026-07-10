using Asp.Versioning;
using Avility.API.Common.Responses;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.Resources.Commands.Create;
using Avility.Application.Resources.Commands.Delete;
using Avility.Application.Resources.Commands.Update;
using Avility.Application.Resources.Dtos;
using Avility.Application.Resources.Queries.GetById;
using Avility.Application.Resources.Queries.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avility.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resources")]
public class ResourcesController : ControllerBase
{
    private readonly ISender _sender;

    public ResourcesController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ResourceDto>>>> Search([FromQuery] SearchResourcesQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ResourceDto>>.SuccessResponse(result));
    }

    [AllowAnonymous]
    [HttpGet("{resourceId:guid}")]
    public async Task<ActionResult<ApiResponse<ResourceDto>>> GetById(Guid resourceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetResourceByIdQuery(resourceId), cancellationToken);
        return Ok(ApiResponse<ResourceDto>.SuccessResponse(result));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ResourceDto>>> Create(CreateResourceCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<ResourceDto>.SuccessResponse(result, "Resource created."));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{resourceId:guid}")]
    public async Task<ActionResult<ApiResponse<ResourceDto>>> Update(Guid resourceId, UpdateResourceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateResourceCommand(resourceId, request.Title, request.Description, request.Url, request.Category);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<ResourceDto>.SuccessResponse(result, "Resource updated."));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{resourceId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid resourceId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteResourceCommand(resourceId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Resource deleted."));
    }
}

public sealed record UpdateResourceRequest(string Title, string Description, string Url, string Category);