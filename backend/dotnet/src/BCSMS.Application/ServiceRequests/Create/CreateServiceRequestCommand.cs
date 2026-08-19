namespace BCSMS.Application.ServiceRequests.Create;

/// <summary>
/// Input command to create a new service request.
/// </summary>
public record CreateServiceRequestCommand(
    Guid CitizenId,
    string Title,
    Guid CategoryId,
    string? Description = null,
    double? Latitude = null,
    double? Longitude = null,
    string? AddressText = null);
