namespace BCSMS.Infrastructure.Persistence.Seeding;

public interface IDbSeeder
{
    Task SeedDevelopmentDataAsync(CancellationToken cancellationToken = default);
}
