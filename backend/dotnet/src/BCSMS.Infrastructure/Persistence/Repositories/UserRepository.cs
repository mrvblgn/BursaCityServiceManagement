using BCSMS.Application.Abstractions.Persistence;
using BCSMS.Application.Reference;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BCSMS.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BcsmsDbContext _dbContext;

    public UserRepository(BcsmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Contact.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .AnyAsync(u => u.Contact.Email == normalizedEmail, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeLookupDto>> GetActiveEmployeesByDepartmentLookupAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.IsActive && u.Role == UserRole.Employee && u.DepartmentId == departmentId)
            .OrderBy(u => u.Name.FirstName)
            .ThenBy(u => u.Name.LastName)
            .Select(u => new EmployeeLookupDto(
                u.Id,
                u.Name.FirstName + " " + u.Name.LastName,
                u.Contact.Email))
            .ToListAsync(cancellationToken);
    }
}
