using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;

namespace BCSMS.Application.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for Category aggregate root.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryLookupDto>> GetActiveLookupAsync(CancellationToken cancellationToken = default);
}
