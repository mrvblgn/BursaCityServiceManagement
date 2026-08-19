using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;

namespace BCSMS.Application.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for User aggregate root.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeLookupDto>> GetActiveEmployeesByDepartmentLookupAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
