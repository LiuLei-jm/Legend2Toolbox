using Legend2Toolbox.Domain.Entities;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<CardNumber> CardNumbers { get; }
    public DbSet<SecurityKey> SecurityKeys { get; }
    public DbSet<CardNumberPath> CardNumberPaths { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
