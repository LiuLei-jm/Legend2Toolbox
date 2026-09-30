using Legend2Toolbox.Domain.Entities.Audit;

namespace Legend2Toolbox.Infrastructure.Auditing;

public sealed class AuditSession
{
    public AuditLog? Current { get; set; }
    public List<AuditBatch> Batches { get; } = [];
    public bool HadRollback { get; set; }

    public void Reset()
    {
        Current = null;
        Batches.Clear();
        HadRollback = false;
    }
}

public sealed class AuditBatch(Guid? transactionId, List<AuditLogDetail> details)
{
    public Guid? TransactionId { get; } = transactionId;
    public List<AuditLogDetail> Details { get; } = details;
    public bool Committed { get; set; } = transactionId is null;
}
