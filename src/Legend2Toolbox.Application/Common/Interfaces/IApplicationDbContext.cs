using Legend2Toolbox.Domain.Entities.Cards;
using Legend2Toolbox.Domain.Entities.ScriptSets;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<CardNumber> CardNumbers { get; }
    public DbSet<ConnectionKey> ConnectionKeys { get; }
    public DbSet<CardNumberPath> CardNumberPaths { get; }
    public DbSet<ScriptSet> ScriptSets { get; }
    public DbSet<ScriptFile> ScriptFiles { get; }
    public DbSet<MaterialFile> MaterialFiles { get; }
    public DbSet<ScriptSetDbData> ScriptSetDbDatas { get; }
    public DbSet<ScriptSegment> ScriptSegments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}