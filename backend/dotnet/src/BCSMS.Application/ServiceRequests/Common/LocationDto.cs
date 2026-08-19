namespace BCSMS.Application.ServiceRequests.Common;

/// <summary>
/// DTO representing geographic location information.
/// </summary>
public record LocationDto(double Latitude, double Longitude, string? AddressText);
