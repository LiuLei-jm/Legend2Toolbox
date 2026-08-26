namespace Legend2Toolbox.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<CardNumber> CardNumbers => Set<CardNumber>();
    public DbSet<ConnectionKey> ConnectionKeys => Set<ConnectionKey>();
    public DbSet<CardNumberPath> CardNumberPaths => Set<CardNumberPath>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        if (Database.IsSqlite())
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    builder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion<string>();
                }
            }
        }
    }
}