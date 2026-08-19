using BCSMS.Application.ServiceRequests.Common;
using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.GetMy;

/// <summary>
/// Summary DTO for service request list views including category name.
/// </summary>
public record ServiceRequestSummaryDto(
    Guid Id,
    string Title,
    Guid CategoryId,
    string CategoryName,
    RequestStatus Status,
    Priority? Priority,
    LocationDto? Location,
    DateTime CreatedAt);
