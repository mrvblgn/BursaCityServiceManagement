using BCSMS.API.Contracts.Manager;
using BCSMS.API.Extensions;
using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.Manager;
using BCSMS.Application.ServiceRequests.Manager.Assign;
using BCSMS.Application.ServiceRequests.Manager.Close;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Application.ServiceRequests.Manager.Reject;
using BCSMS.Application.ServiceRequests.Manager.Reopen;
using BCSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCSMS.API.Controllers;

[ApiController]
[Route("api/manager/service-requests")]
[Authorize(Roles = "Manager")]
public class ManagerServiceRequestsController : ControllerBase
{
    private readonly IManagerServiceRequestService _managerService;

    public ManagerServiceRequestsController(IManagerServiceRequestService managerService)
    {
        _managerService = managerService;
    }

    /// <summary>
    /// Retrieves paginated municipal service requests with optional filters for status, category, department, and priority.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MunicipalServiceRequestSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMunicipalRequests(
        [FromQuery] RequestStatus? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Priority? priority = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var managerUserId = User.GetUserId();

        var query = new GetMunicipalRequestsQuery(
            managerUserId,
            status,
            categoryId,
            departmentId,
            priority,
            pageNumber,
            pageSize);

        var result = await _managerService.GetMunicipalRequestsAsync(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves complete details of any municipal service request.
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
        var managerUserId = User.GetUserId();

        var result = await _managerService.GetMunicipalRequestByIdAsync(id, managerUserId, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Transitions a service request from New to Reviewing.
    /// </summary>
    [HttpPost("{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartReview(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var managerUserId = User.GetUserId();

        await _managerService.StartReviewAsync(id, managerUserId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Assigns a reviewing service request to a department, employee, and priority.
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Assign(
        [FromRoute] Guid id,
        [FromBody] AssignRequestApiRequest request,
        CancellationToken cancellationToken)
    {
        var managerUserId = User.GetUserId();

        var command = new AssignRequestCommand(
            id,
            request.DepartmentId,
            request.EmployeeId,
            request.Priority,
            managerUserId);

        await _managerService.AssignRequestAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Rejects a service request in New or Reviewing status with an optional note.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id,
        [FromBody] WorkflowNoteApiRequest? request,
        CancellationToken cancellationToken)
    {
        var managerUserId = User.GetUserId();

        var command = new RejectRequestCommand(id, request?.Note, managerUserId);

        await _managerService.RejectRequestAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Closes a service request in Resolved status with an optional confirmation note.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Close(
        [FromRoute] Guid id,
        [FromBody] WorkflowNoteApiRequest? request,
        CancellationToken cancellationToken)
    {
        var managerUserId = User.GetUserId();

        var command = new CloseRequestCommand(id, request?.Note, managerUserId);

        await _managerService.CloseRequestAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reopens a service request in Resolved status back to InProgress with an optional note.
    /// </summary>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reopen(
        [FromRoute] Guid id,
        [FromBody] WorkflowNoteApiRequest? request,
        CancellationToken cancellationToken)
    {
        var managerUserId = User.GetUserId();

        var command = new ReopenRequestCommand(id, request?.Note, managerUserId);

        await _managerService.ReopenRequestAsync(command, cancellationToken);

        return NoContent();
    }
}
