
using Legend2Toolbox.Domain.Entities.ScriptSets;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.ScriptSetsConfiguration;

public class MaterialFileConfiguration : IEntityTypeConfiguration<MaterialFile>
{
    public void Configure(EntityTypeBuilder<MaterialFile> builder)
    {
        builder.ToTable("MaterialFiles");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FileName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.TargetPath).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Password).IsRequired().HasMaxLength(200);
    }
}
