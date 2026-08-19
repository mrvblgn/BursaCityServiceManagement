namespace BCSMS.Application.Common.Exceptions;

/// <summary>
/// Thrown when application request validation fails.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
