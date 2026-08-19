using BCSMS.Domain.Enums;

namespace BCSMS.Application.Auth.Register;

/// <summary>
/// Response returned after successfully registering a new user.
/// Contains no sensitive security fields.
/// </summary>
public record RegisterResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    DateTime CreatedAt);
