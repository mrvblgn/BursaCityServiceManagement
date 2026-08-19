using BCSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BCSMS.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the BCSMS platform.
/// Exposes DbSets only for Aggregate Roots.
/// Child entities (StatusHistoryEntry, Comment, Attachment) are managed through the ServiceRequest aggregate.
/// </summary>
public class BcsmsDbContext : DbContext
{
    public BcsmsDbContext(DbContextOptions<BcsmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BcsmsDbContext).Assembly);
    }
}
