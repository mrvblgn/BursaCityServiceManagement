using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BCSMS.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly BcsmsDbContext _dbContext;

    public DepartmentRepository(BcsmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentLookupDto>> GetActiveLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentLookupDto(d.Id, d.Name))
            .ToListAsync(cancellationToken);
    }
}
