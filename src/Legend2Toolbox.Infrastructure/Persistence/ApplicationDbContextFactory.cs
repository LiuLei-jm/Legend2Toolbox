using Microsoft.EntityFrameworkCore.Design;

namespace Legend2Toolbox.Infrastructure.Persistence;

// Creating a migration must not start the API, its hosted services, or migrate the live database.
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite("Data Source=design-time.db").Options);
}
