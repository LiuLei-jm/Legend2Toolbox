using Legend2Toolbox.Domain.Entities.Audit;

namespace Legend2Toolbox.Infrastructure.Persistence.EntitiesConfiguration;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TraceId).HasMaxLength(128);
        builder.Property(x => x.ActorUserName).HasMaxLength(256);
        builder.Property(x => x.TargetName).HasMaxLength(256);
        builder.Property(x => x.TargetId).HasMaxLength(128);
        builder.Property(x => x.ActorType).HasMaxLength(32);
        builder.Property(x => x.Module).HasMaxLength(64);
        builder.Property(x => x.Action).HasMaxLength(64);
        builder.Property(x => x.ClientIp).HasMaxLength(64);
        builder.Property(x => x.PeerIp).HasMaxLength(64);
        builder.Property(x => x.Outcome).HasMaxLength(32);
        builder.Property(x => x.DataStatus).HasMaxLength(32);
        builder.Property(x => x.ErrorCode).HasMaxLength(128);
        builder.HasIndex(x => x.OccurredAtUnixMs);
        builder.HasIndex(x => new { x.ActorUserId, x.OccurredAtUnixMs });
        builder.HasIndex(x => new { x.SubjectUserId, x.Action, x.OccurredAtUnixMs });
        builder.HasIndex(x => new { x.Module, x.TargetId, x.OccurredAtUnixMs });
        builder.HasMany(x => x.Details).WithOne().HasForeignKey(x => x.AuditLogId)
            .OnDelete(DeleteBehavior.Cascade);
        // Actor/subject IDs are historical values, not foreign keys to deletable users.
    }
}

public sealed class AuditLogDetailConfiguration : IEntityTypeConfiguration<AuditLogDetail>
{
    public void Configure(EntityTypeBuilder<AuditLogDetail> builder)
    {
        builder.ToTable("AuditLogDetails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(128);
        builder.Property(x => x.EntityId).HasMaxLength(256);
        builder.Property(x => x.ChangeType).HasMaxLength(32);
        builder.HasIndex(x => new { x.AuditLogId, x.Sequence }).IsUnique();
    }
}
