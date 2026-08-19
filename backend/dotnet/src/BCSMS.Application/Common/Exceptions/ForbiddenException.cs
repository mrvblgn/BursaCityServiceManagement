namespace BCSMS.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated user is forbidden from performing the operation.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
