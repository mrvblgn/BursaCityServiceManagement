using BCSMS.API.Contracts.ServiceRequests;
using BCSMS.API.Extensions;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;
using BCSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCSMS.API.Controllers;

[ApiController]
[Route("api/service-requests")]
[Authorize(Roles = "Citizen")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestsController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    /// <summary>
    /// Creates a new municipal service request for the authenticated citizen.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateServiceRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceRequestApiRequest request,
        CancellationToken cancellationToken)
    {
        var citizenId = User.GetUserId();

        var command = new CreateServiceRequestCommand(
            citizenId,
            request.Title,
            request.CategoryId,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.AddressText);

        var response = await _serviceRequestService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Retrieves paginated service requests created by the authenticated citizen.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<ServiceRequestSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] RequestStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var citizenId = User.GetUserId();

        var query = new GetMyServiceRequestsQuery(citizenId, status, pageNumber, pageSize);

        var result = await _serviceRequestService.GetMyRequestsAsync(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information for a specific service request.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServiceRequestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var requestingUserId = User.GetUserId();

        var query = new GetServiceRequestByIdQuery(id, requestingUserId);

        var result = await _serviceRequestService.GetByIdAsync(query, cancellationToken);

        return Ok(result);
    }
}
