using BCSMS.Domain.Entities;

namespace BCSMS.Application.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for Department aggregate root.
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// Retrieves a Department by ID.
    /// </summary>
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
