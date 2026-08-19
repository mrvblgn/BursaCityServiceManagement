namespace BCSMS.API.Contracts.ServiceRequests;

/// <summary>
/// API contract for creating a service request.
/// CitizenId is not provided here; it is extracted securely from authenticated JWT claims.
/// </summary>
public record CreateServiceRequestApiRequest(
    string Title,
    Guid CategoryId,
    string? Description = null,
    double? Latitude = null,
    double? Longitude = null,
    string? AddressText = null);
