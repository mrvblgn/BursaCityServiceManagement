using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Employee.GetAssigned;
using BCSMS.Application.ServiceRequests.Employee.Resolve;
using BCSMS.Application.ServiceRequests.GetById;

namespace BCSMS.Application.ServiceRequests.Employee;

public interface IEmployeeServiceRequestService
{
    Task<PagedResult<EmployeeServiceRequestSummaryDto>> GetMyAssignedRequestsAsync(
        GetMyAssignedRequestsQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailDto> GetAssignedRequestByIdAsync(
        Guid requestId,
        Guid employeeUserId,
        CancellationToken cancellationToken = default);

    Task StartWorkAsync(
        Guid requestId,
        Guid employeeUserId,
        CancellationToken cancellationToken = default);

    Task ResolveRequestAsync(
        ResolveRequestCommand command,
        CancellationToken cancellationToken = default);
}
