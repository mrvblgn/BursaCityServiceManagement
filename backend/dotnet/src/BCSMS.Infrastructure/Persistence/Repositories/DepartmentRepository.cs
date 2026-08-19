using BCSMS.Application.Abstractions.Persistence;
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
}
