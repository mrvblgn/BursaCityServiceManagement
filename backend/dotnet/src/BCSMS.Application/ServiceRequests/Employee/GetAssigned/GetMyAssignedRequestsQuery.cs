using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Employee.GetAssigned;

public record GetMyAssignedRequestsQuery(
    Guid EmployeeUserId,
    RequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10);
