using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.GetMy;

/// <summary>
/// Query for retrieving paginated service requests for a citizen with optional status filtering.
/// </summary>
public record GetMyServiceRequestsQuery(
    Guid CitizenId,
    RequestStatus? StatusFilter = null,
    int PageNumber = 1,
    int PageSize = 10);
