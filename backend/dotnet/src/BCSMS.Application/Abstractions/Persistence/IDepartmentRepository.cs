using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;

namespace BCSMS.Application.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for Department aggregate root.
/// </summary>
public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentLookupDto>> GetActiveLookupAsync(CancellationToken cancellationToken = default);
}
