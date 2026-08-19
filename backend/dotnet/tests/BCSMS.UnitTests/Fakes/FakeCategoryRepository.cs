using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;

namespace BCSMS.UnitTests.Fakes;

public class FakeCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<Guid, Category> _categories = new();

    public void Add(Category category) => _categories[category.Id] = category;

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _categories.TryGetValue(id, out var category);
        return Task.FromResult(category);
    }

    public Task<IReadOnlyList<CategoryLookupDto>> GetActiveLookupAsync(CancellationToken cancellationToken = default)
    {
        var list = _categories.Values
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryLookupDto(c.Id, c.Name, c.Description))
            .ToList();

        return Task.FromResult<IReadOnlyList<CategoryLookupDto>>(list);
    }
}
