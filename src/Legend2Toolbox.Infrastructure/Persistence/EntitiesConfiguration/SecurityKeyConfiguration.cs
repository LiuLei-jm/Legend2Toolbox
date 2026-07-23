namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

public class SecurityKeyConfiguration : IEntityTypeConfiguration<SecurityKey>
{
    public void Configure(EntityTypeBuilder<SecurityKey> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key)
            .HasMaxLength(512);
        builder.HasIndex(x => x.UserId)
            .IsUnique();
        builder.HasIndex(x => x.Key)
            .IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithOne(u => u.SecurityKey)
            .HasForeignKey<SecurityKey>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
