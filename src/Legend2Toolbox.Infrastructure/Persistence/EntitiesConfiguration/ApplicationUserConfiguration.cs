namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.Property(u => u.NickName)
            .HasMaxLength(128);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}