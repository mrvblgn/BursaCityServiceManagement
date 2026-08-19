namespace BCSMS.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication fails (e.g. invalid credentials or missing identity).
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid email or password.")
        : base(message)
    {
    }
}
