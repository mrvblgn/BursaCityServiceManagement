using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.Create;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.GetMy;

namespace BCSMS.Application.ServiceRequests;

/// <summary>
/// Service interface for ServiceRequest use cases.
/// </summary>
public interface IServiceRequestService
{
    Task<CreateServiceRequestResponse> CreateAsync(
        CreateServiceRequestCommand command,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ServiceRequestSummaryDto>> GetMyRequestsAsync(
        GetMyServiceRequestsQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailDto> GetByIdAsync(
        GetServiceRequestByIdQuery query,
        CancellationToken cancellationToken = default);
}
