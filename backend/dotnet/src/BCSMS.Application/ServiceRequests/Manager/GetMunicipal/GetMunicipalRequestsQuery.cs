using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Manager.GetMunicipal;

public record GetMunicipalRequestsQuery(
    Guid ManagerUserId,
    RequestStatus? Status = null,
    Guid? CategoryId = null,
    Guid? DepartmentId = null,
    Priority? Priority = null,
    int PageNumber = 1,
    int PageSize = 10);
