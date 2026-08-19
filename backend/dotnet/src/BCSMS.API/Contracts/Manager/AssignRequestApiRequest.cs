using BCSMS.Domain.Enums;

namespace BCSMS.API.Contracts.Manager;

/// <summary>
/// API contract for assigning a service request to a department and employee.
/// Manager ID is extracted from authenticated JWT claims.
/// </summary>
public record AssignRequestApiRequest(
    Guid DepartmentId,
    Guid EmployeeId,
    Priority Priority);
