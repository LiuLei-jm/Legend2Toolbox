
using Legend2Toolbox.Domain.Entities.ScriptSets;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.ScriptSetsConfiguration;

public class ScriptSegmentConfiguration : IEntityTypeConfiguration<ScriptSegment>
{
    public void Configure(EntityTypeBuilder<ScriptSegment> builder)
    {
        builder.ToTable("ScriptSegments");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TriggerField).IsRequired().HasMaxLength(200);
    }
}
