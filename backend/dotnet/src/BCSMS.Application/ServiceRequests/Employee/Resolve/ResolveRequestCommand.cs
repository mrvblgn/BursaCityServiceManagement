namespace BCSMS.Application.ServiceRequests.Employee.Resolve;

public record ResolveRequestCommand(
    Guid RequestId,
    string? Note,
    Guid EmployeeUserId);
