namespace BCSMS.API.Contracts.Auth;

/// <summary>
/// API contract for user login.
/// </summary>
public record LoginRequest(string Email, string Password);
