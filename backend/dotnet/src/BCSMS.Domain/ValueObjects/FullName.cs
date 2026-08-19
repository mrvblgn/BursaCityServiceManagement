using BCSMS.Domain.Common;

namespace BCSMS.Domain.ValueObjects;

/// <summary>
/// Represents a person's full name.
/// </summary>
public record FullName
{
    public string FirstName { get; }
    public string LastName { get; }

    public FullName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public override string ToString() => $"{FirstName} {LastName}";
}
