using BCSMS.Application.Abstractions.Security;
using BCSMS.Application.Abstractions.Time;
using BCSMS.Domain.Entities;
using BCSMS.Domain.Enums;
using BCSMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BCSMS.Infrastructure.Persistence.Seeding;

public class DbSeeder : IDbSeeder
{
    private readonly BcsmsDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        BcsmsDbContext dbContext,
        IPasswordHasher passwordHasher,
        IClock clock,
        ILogger<DbSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _clock = clock;
        _logger = logger;
    }

    public async Task SeedDevelopmentDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting development seed data initialization...");

        var now = _clock.UtcNow;

        // 1. Departments
        var fenIsleriId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var parkBahcelerId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var temizlikIsleriId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var ulasimId = Guid.Parse("10000000-0000-0000-0000-000000000004");

        var departments = new List<Department>
        {
            new(fenIsleriId, "Fen İşleri", "Yol, asfalt ve altyapı işleri", now),
            new(parkBahcelerId, "Park ve Bahçeler", "Park, bahçe ve yeşil alan bakım işleri", now),
            new(temizlikIsleriId, "Temizlik İşleri", "Çevre temizliği ve atık yönetimi", now),
            new(ulasimId, "Ulaşım", "Trafik levhaları, sinyalizasyon ve toplu taşıma", now)
        };

        foreach (var dept in departments)
        {
            if (!await _dbContext.Departments.AnyAsync(d => d.Id == dept.Id || d.Name == dept.Name, cancellationToken))
            {
                await _dbContext.Departments.AddAsync(dept, cancellationToken);
            }
        }

        // 2. Categories
        var roadCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var lightingCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var wasteCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        var parksCategoryId = Guid.Parse("20000000-0000-0000-0000-000000000004");

        var categories = new List<Category>
        {
            new(roadCategoryId, "Road and Pavement", "Potholes, sidewalk and asphalt damage", now),
            new(lightingCategoryId, "Street Lighting and Electrical", "Faulty street lights and traffic signals", now),
            new(wasteCategoryId, "Waste and Cleaning", "Garbage collection and litter cleanup", now),
            new(parksCategoryId, "Parks and Green Areas", "Damaged benches, playground equipment, and grass maintenance", now)
        };

        foreach (var cat in categories)
        {
            if (!await _dbContext.Categories.AnyAsync(c => c.Id == cat.Id || c.Name == cat.Name, cancellationToken))
            {
                await _dbContext.Categories.AddAsync(cat, cancellationToken);
            }
        }

        // 3. Demo Users (all with hashed password "Demo12345!")
        var defaultPasswordHash = _passwordHasher.HashPassword("Demo12345!");

        var managerId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var employee1Id = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var employee2Id = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var adminId = Guid.Parse("30000000-0000-0000-0000-000000000004");

        var users = new List<User>
        {
            new(
                managerId,
                new FullName("Kemal", "Yilmaz"),
                new ContactInfo("manager@bursa.bel.tr", "5551000001"),
                defaultPasswordHash,
                UserRole.Manager,
                fenIsleriId,
                now),
            new(
                employee1Id,
                new FullName("Ahmet", "Usta"),
                new ContactInfo("employee1@bursa.bel.tr", "5551000002"),
                defaultPasswordHash,
                UserRole.Employee,
                fenIsleriId,
                now),
            new(
                employee2Id,
                new FullName("Mehmet", "Bahcivan"),
                new ContactInfo("employee2@bursa.bel.tr", "5551000003"),
                defaultPasswordHash,
                UserRole.Employee,
                parkBahcelerId,
                now),
            new(
                adminId,
                new FullName("Sistem", "Yoneticisi"),
                new ContactInfo("admin@bursa.bel.tr", "5551000004"),
                defaultPasswordHash,
                UserRole.Admin,
                null,
                now)
        };

        foreach (var user in users)
        {
            if (!await _dbContext.Users.AnyAsync(u => u.Id == user.Id || u.Contact.Email == user.Contact.Email, cancellationToken))
            {
                await _dbContext.Users.AddAsync(user, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Development seed data initialization completed successfully.");
    }
}
