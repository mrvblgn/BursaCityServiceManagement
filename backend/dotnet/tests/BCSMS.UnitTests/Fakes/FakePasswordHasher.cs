using BCSMS.Application.Abstractions.Security;

namespace BCSMS.UnitTests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hashed_{password}";

    public bool VerifyPassword(string password, string passwordHash) =>
        passwordHash == $"hashed_{password}" || passwordHash.Contains(password);
}
