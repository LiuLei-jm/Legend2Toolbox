namespace Legend2Toolbox.Domain.Entities.Audit;

public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TraceId { get; set; } = string.Empty;
    // Unix milliseconds keep ordering/filtering identical on SQLite and PostgreSQL.
    public long OccurredAtUnixMs { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public Guid? ActorUserId { get; set; }
    public string? ActorUserName { get; set; }
    public Guid? SubjectUserId { get; set; }
    public string ActorType { get; set; } = "User";
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? TargetName { get; set; }
    public string? ClientIp { get; set; }
    public string? PeerIp { get; set; }
    public string Outcome { get; set; } = "Failed";
    public string DataStatus { get; set; } = "NoChanges";
    public string? ErrorCode { get; set; }
    public bool CaptureIncomplete { get; set; }
    public List<AuditLogDetail> Details { get; set; } = [];
}

public sealed class AuditLogDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AuditLogId { get; set; }
    public int Sequence { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string OldValues { get; set; } = "{}";
    public string NewValues { get; set; } = "{}";
}
