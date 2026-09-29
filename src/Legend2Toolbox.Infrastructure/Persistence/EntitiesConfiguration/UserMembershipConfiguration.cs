namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

public class UserMembershipConfiguration : IEntityTypeConfiguration<UserMembership>
{
    public void Configure(EntityTypeBuilder<UserMembership> builder)
    {
        builder.ToTable("UserMemberships");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserMembership>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
