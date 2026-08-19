namespace BCSMS.Domain.Common;

/// <summary>
/// Exception thrown when a domain business rule is violated.
/// Intended to be caught at the API layer and mapped to HTTP 400/422.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
