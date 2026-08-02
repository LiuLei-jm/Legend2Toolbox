namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

public class CardNumberConfiguration : IEntityTypeConfiguration<CardNumber>
{
    public void Configure(EntityTypeBuilder<CardNumber> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Owner)
            .HasMaxLength(200);
        builder.Property(c => c.Cdk)
            .HasMaxLength(200);
        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.CardNumbers)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
