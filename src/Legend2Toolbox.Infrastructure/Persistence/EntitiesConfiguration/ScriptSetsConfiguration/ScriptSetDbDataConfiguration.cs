
using Legend2Toolbox.Domain.Entities.ScriptSets;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.ScriptSetsConfiguration;

public class ScriptSetDbDataConfiguration : IEntityTypeConfiguration<ScriptSetDbData>
{
    public void Configure(EntityTypeBuilder<ScriptSetDbData> builder)
    {
        builder.ToTable("ScriptSetDbDatas");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(d => d.DataJson)
            .IsRequired();

        builder.HasIndex(d => new { d.ScriptSetId, d.TableType });
        builder.HasOne(d => d.ScriptSet)
            .WithMany(s => s.DbDatas)
            .HasForeignKey(d => d.ScriptSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
