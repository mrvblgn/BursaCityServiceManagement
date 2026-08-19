using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Reference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCSMS.API.Controllers;

[ApiController]
[Route("api")]
public class ReferenceDataController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserRepository _userRepository;

    public ReferenceDataController(
        ICategoryRepository categoryRepository,
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository)
    {
        _categoryRepository = categoryRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Retrieves active service request categories for dropdown selection.
    /// </summary>
    [HttpGet("categories")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetActiveLookupAsync(cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves active municipal departments for assignment and filtering.
    /// </summary>
    [HttpGet("departments")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await _departmentRepository.GetActiveLookupAsync(cancellationToken);
        return Ok(departments);
    }

    /// <summary>
    /// Retrieves active employees belonging to the specified department.
    /// </summary>
    [HttpGet("departments/{departmentId:guid}/employees")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDepartmentEmployees(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        var employees = await _userRepository.GetActiveEmployeesByDepartmentLookupAsync(departmentId, cancellationToken);
        return Ok(employees);
    }
}
