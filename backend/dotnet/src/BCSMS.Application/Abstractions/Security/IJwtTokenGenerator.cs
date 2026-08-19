using BCSMS.Domain.Entities;

namespace BCSMS.Application.Abstractions.Security;

/// <summary>
/// Abstraction for generating JWT access tokens.
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
