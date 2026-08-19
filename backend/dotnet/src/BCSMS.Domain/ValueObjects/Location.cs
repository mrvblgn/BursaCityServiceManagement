using BCSMS.Domain.Common;

namespace BCSMS.Domain.ValueObjects;

/// <summary>
/// Represents a geographic location with coordinates and an optional human-readable address.
/// </summary>
public record Location
{
    public double Latitude { get; }
    public double Longitude { get; }
    public string? AddressText { get; }

    public Location(double latitude, double longitude, string? addressText = null)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException($"Latitude must be between -90 and 90. Got: {latitude}");

        if (longitude < -180 || longitude > 180)
            throw new DomainException($"Longitude must be between -180 and 180. Got: {longitude}");

        Latitude = latitude;
        Longitude = longitude;
        AddressText = addressText;
    }
}
