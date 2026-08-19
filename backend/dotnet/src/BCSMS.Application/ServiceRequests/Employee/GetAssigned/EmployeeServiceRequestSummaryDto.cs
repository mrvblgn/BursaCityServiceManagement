using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Employee.GetAssigned;

/// <summary>
/// Summary projection of a service request assigned to an employee.
/// </summary>
public record EmployeeServiceRequestSummaryDto(
    Guid Id,
    string Title,
    Guid CategoryId,
    string CategoryName,
    RequestStatus Status,
    Priority? Priority,
    LocationDto? Location,
    DateTime CreatedAt);
