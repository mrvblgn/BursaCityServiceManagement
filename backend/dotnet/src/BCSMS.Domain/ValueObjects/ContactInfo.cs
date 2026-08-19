using BCSMS.Domain.Common;

namespace BCSMS.Domain.ValueObjects;

/// <summary>
/// Represents a user's contact information.
/// </summary>
public record ContactInfo
{
    public string Email { get; }
    public string? PhoneNumber { get; }

    public ContactInfo(string email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
    }
}
