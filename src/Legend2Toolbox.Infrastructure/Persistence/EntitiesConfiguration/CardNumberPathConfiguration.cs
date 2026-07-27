using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

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
