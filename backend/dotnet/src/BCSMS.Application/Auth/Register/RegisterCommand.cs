namespace BCSMS.Application.Auth.Register;

/// <summary>
/// Command to register a new citizen user.
/// </summary>
public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password);
