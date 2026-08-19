using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;

namespace BCSMS.UnitTests.Fakes;

public class FakeDepartmentRepository : IDepartmentRepository
{
    public readonly Dictionary<Guid, Department> Departments = new();

    public void Add(Department department) => Departments[department.Id] = department;

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Departments.TryGetValue(id, out var department);
        return Task.FromResult(department);
    }

    public Task<IReadOnlyList<DepartmentLookupDto>> GetActiveLookupAsync(CancellationToken cancellationToken = default)
    {
        var list = Departments.Values
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentLookupDto(d.Id, d.Name))
            .ToList();

        return Task.FromResult<IReadOnlyList<DepartmentLookupDto>>(list);
    }
}
