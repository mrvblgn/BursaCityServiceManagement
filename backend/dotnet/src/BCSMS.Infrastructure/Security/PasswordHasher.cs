using BCSMS.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;

namespace BCSMS.Infrastructure.Security;

/// <summary>
/// Password hashing implementation using ASP.NET Core Identity PasswordHasher.
/// Uses standard PBKDF2 with HMAC-SHA256 and secure cryptographic salting.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object DummyUser = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(DummyUser, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(DummyUser, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
