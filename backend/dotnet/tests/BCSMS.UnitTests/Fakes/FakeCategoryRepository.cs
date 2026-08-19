using BCSMS.Application.Abstractions.Persistence;
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
}
