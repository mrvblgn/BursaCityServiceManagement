using BCSMS.Domain.Entities;

namespace BCSMS.Application.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for User aggregate root.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
