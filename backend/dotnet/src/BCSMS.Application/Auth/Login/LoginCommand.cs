namespace BCSMS.Application.Auth.Login;

/// <summary>
/// Command to authenticate an existing user.
/// </summary>
public record LoginCommand(string Email, string Password);
