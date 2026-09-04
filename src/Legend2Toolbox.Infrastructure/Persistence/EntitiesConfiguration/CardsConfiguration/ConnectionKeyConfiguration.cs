using Legend2Toolbox.Domain.Entities.Cards;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.CardsConfiguration;

public class ConnectionKeyConfiguration : IEntityTypeConfiguration<ConnectionKey>
{
    public void Configure(EntityTypeBuilder<ConnectionKey> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key)
            .HasMaxLength(2048);
        builder.HasIndex(x => x.UserId)
            .IsUnique();
        builder.HasIndex(x => x.Key)
            .IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithOne(u => u.ConnectionKey)
            .HasForeignKey<ConnectionKey>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}