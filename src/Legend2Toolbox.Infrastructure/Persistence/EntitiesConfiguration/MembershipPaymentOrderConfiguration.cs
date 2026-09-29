namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

public class MembershipPaymentOrderConfiguration : IEntityTypeConfiguration<MembershipPaymentOrder>
{
    public void Configure(EntityTypeBuilder<MembershipPaymentOrder> builder)
    {
        builder.ToTable("MembershipPaymentOrders");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.CreatedOn });
        builder.Property(x => x.OrderId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
