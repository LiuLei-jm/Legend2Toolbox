namespace Legend2Toolbox.Application.Common.Interfaces;

public record AuditOperation(string Module, string Action, string? TargetId = null,
    string? TargetName = null, string? ActorType = null);

public interface IAuditService
{
    Task<T> ExecuteAsync<T>(AuditOperation operation, Func<Task<T>> action);
    void IdentifySubject(Guid userId, string? userName);
}
