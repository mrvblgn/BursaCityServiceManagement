using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;

namespace BCSMS.UnitTests.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = new();

    public void Add(User user) => _users[user.Id] = user;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = _users.Values.FirstOrDefault(u => u.Contact.Email == normalizedEmail);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = _users.Values.Any(u => u.Contact.Email == normalizedEmail);
        return Task.FromResult(exists);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EmployeeLookupDto>> GetActiveEmployeesByDepartmentLookupAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var list = _users.Values
            .Where(u => u.IsActive && u.Role == UserRole.Employee && u.DepartmentId == departmentId)
            .OrderBy(u => u.Name.FirstName)
            .ThenBy(u => u.Name.LastName)
            .Select(u => new EmployeeLookupDto(
                u.Id,
                u.Name.FirstName + " " + u.Name.LastName,
                u.Contact.Email))
            .ToList();

        return Task.FromResult<IReadOnlyList<EmployeeLookupDto>>(list);
    }
}
