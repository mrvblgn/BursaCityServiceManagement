using BCSMS.Application.Common.Models;
using BCSMS.Application.ServiceRequests.GetById;
using BCSMS.Application.ServiceRequests.Manager.Assign;
using BCSMS.Application.ServiceRequests.Manager.Close;
using BCSMS.Application.ServiceRequests.Manager.GetMunicipal;
using BCSMS.Application.ServiceRequests.Manager.Reject;
using BCSMS.Application.ServiceRequests.Manager.Reopen;

namespace BCSMS.Application.ServiceRequests.Manager;

public interface IManagerServiceRequestService
{
    Task<PagedResult<MunicipalServiceRequestSummaryDto>> GetMunicipalRequestsAsync(
        GetMunicipalRequestsQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailDto> GetMunicipalRequestByIdAsync(
        Guid requestId,
        Guid managerUserId,
        CancellationToken cancellationToken = default);

    Task StartReviewAsync(
        Guid requestId,
        Guid managerUserId,
        CancellationToken cancellationToken = default);

    Task AssignRequestAsync(
        AssignRequestCommand command,
        CancellationToken cancellationToken = default);

    Task RejectRequestAsync(
        RejectRequestCommand command,
        CancellationToken cancellationToken = default);

    Task CloseRequestAsync(
        CloseRequestCommand command,
        CancellationToken cancellationToken = default);

    Task ReopenRequestAsync(
        ReopenRequestCommand command,
        CancellationToken cancellationToken = default);
}
