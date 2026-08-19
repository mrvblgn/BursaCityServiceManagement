namespace BCSMS.API.Contracts.Auth;

/// <summary>
/// API contract for public citizen registration.
/// Does not accept Role, DepartmentId, or IsActive from client.
/// </summary>
public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password);
