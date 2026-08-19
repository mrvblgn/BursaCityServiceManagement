namespace BCSMS.Application.Common.Exceptions;

/// <summary>
/// Thrown when an operation cannot be performed due to the current state of a resource (e.g. inactive user/category).
/// </summary>
public class ApplicationConflictException : Exception
{
    public ApplicationConflictException(string message)
        : base(message)
    {
    }
}
