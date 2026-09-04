using Legend2Toolbox.Domain.Entities.ScriptSets;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.ScriptSetsConfiguration;

public class ScriptSetConfiguration : IEntityTypeConfiguration<ScriptSet>
{
    public void Configure(EntityTypeBuilder<ScriptSet> builder)
    {
        builder.ToTable("ScriptSets");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(s => s.ScriptFiles)
            .WithOne(f => f.ScriptSet)
            .HasForeignKey(f => f.ScriptSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.MaterialFiles)
            .WithOne(m => m.ScriptSet)
            .HasForeignKey(m => m.ScriptSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
