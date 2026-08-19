using BCSMS.API.Contracts.Employee;
using BCSMS.API.Extensions;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Employee;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.Employee.Resolve;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCSMS.API.Controllers;

[ApiController]
[Route("api/employee/service-requests")]
[Authorize(Roles = "Employee")]
public class EmployeeServiceRequestsController : ControllerBase
{
    private readonly IEmployeeServiceRequestService _employeeService;

    public EmployeeServiceRequestsController(IEmployeeServiceRequestService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Retrieves paginated service requests assigned specifically to the authenticated employee.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeServiceRequestSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyAssignedRequests(
        [FromQuery] RequestStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var employeeUserId = User.GetUserId();

        var query = new GetMyAssignedRequestsQuery(
            employeeUserId,
            status,
            pageNumber,
            pageSize);

        var result = await _employeeService.GetMyAssignedRequestsAsync(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves details of a service request assigned to the authenticated employee.
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
        var employeeUserId = User.GetUserId();

        var result = await _employeeService.GetAssignedRequestByIdAsync(id, employeeUserId, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Begins progress on an assigned service request (Assigned -> InProgress).
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartWork(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var employeeUserId = User.GetUserId();

        await _employeeService.StartWorkAsync(id, employeeUserId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Marks an in-progress service request as resolved with an optional note (InProgress -> Resolved).
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Resolve(
        [FromRoute] Guid id,
        [FromBody] ResolveRequestApiRequest? request,
        CancellationToken cancellationToken)
    {
        var employeeUserId = User.GetUserId();

        var command = new ResolveRequestCommand(id, request?.Note, employeeUserId);

        await _employeeService.ResolveRequestAsync(command, cancellationToken);

        return NoContent();
    }
}
