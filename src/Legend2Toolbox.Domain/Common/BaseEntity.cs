namespace Legend2Toolbox.Domain.Common;

public class BaseEntity
{
    public Guid Id { get; protected set; } = default!;
}

public class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedOn { get; protected set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTimeOffset? LastModifiedOn { get; protected set; }
    public string? LastModifiedBy { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;
    public void MarkDeleted(string deletedBy)
    {
        IsDeleted = true;
        LastModifiedOn = DateTimeOffset.UtcNow;
        LastModifiedBy = deletedBy;
    }
}
