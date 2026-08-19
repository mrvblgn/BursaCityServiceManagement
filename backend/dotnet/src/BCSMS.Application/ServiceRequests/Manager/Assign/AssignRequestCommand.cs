using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Manager.Assign;

public record AssignRequestCommand(
    Guid RequestId,
    Guid DepartmentId,
    Guid EmployeeId,
    Priority Priority,
    Guid ManagerUserId);
