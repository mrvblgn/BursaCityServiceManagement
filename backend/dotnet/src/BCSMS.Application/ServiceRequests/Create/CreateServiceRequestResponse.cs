using BCSMS.Domain.Enums;

namespace BCSMS.Application.ServiceRequests.Create;

/// <summary>
/// Response returned after successfully creating a service request.
/// </summary>
public record CreateServiceRequestResponse(
    Guid Id,
    string Title,
    Guid CategoryId,
    Guid CitizenId,
    RequestStatus Status,
    DateTime CreatedAt);
