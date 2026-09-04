using Legend2Toolbox.Domain.Entities.Cards;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration.CardsConfiguration;

public class CardNumberPathConfiguration : IEntityTypeConfiguration<CardNumberPath>
{
    public void Configure(EntityTypeBuilder<CardNumberPath> builder)
    {
        builder.Property(x => x.BasePath)
            .HasMaxLength(512);
        builder.Property(x => x.FileName)
            .HasMaxLength(256);
        builder.HasOne<ApplicationUser>()
            .WithOne(x => x.CardNumberPath)
            .HasForeignKey<CardNumberPath>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}