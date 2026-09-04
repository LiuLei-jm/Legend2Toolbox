
using Legend2Toolbox.Domain.Entities.ScriptSets;
using Org.BouncyCastle.Pkix;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.ScriptSetsConfiguration;

public class ScriptFileConfiguration : IEntityTypeConfiguration<ScriptFile>
{
    public void Configure(EntityTypeBuilder<ScriptFile> builder)
    {
        builder.ToTable("ScriptFiles");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FileName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.FilePath).IsRequired().HasMaxLength(200);

        builder.HasMany(s => s.Segments)
            .WithOne(seg => seg.ScriptFile)
            .HasForeignKey(seg => seg.ScriptFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
