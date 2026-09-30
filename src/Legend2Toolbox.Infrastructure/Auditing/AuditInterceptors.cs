using System.Data.Common;
using Legend2Toolbox.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Legend2Toolbox.Infrastructure.Auditing;

public sealed class AuditSaveChangesInterceptor(AuditSession session, ILogger<AuditSaveChangesInterceptor> logger)
    : SaveChangesInterceptor
{
    private readonly Dictionary<Guid, List<AuditLogDetail>> _pending = [];

    private async Task CaptureAsync(DbContext? context, bool async, CancellationToken cancellationToken)
    {
        if (session.Current is null || context is null) return;
        try
        {
            _pending[context.ContextId.InstanceId] = await AuditSnapshot.CaptureAsync(context, async, cancellationToken);
        }
        catch (Exception ex)
        {
            session.Current.CaptureIncomplete = true;
            logger.LogError(ex, "审计变更采集失败，操作 {AuditId}", session.Current.Id);
        }
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureAsync(eventData.Context, false, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await CaptureAsync(eventData.Context, true, cancellationToken);
        return result;
    }

    private void Saved(DbContext? context)
    {
        if (context is null || !_pending.Remove(context.ContextId.InstanceId, out var details) || session.Current is null) return;
        if (details.Count > 0)
            session.Batches.Add(new AuditBatch(context.Database.CurrentTransaction?.TransactionId, details));
    }
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Saved(eventData.Context);
        return result;
    }
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        Saved(eventData.Context);
        return ValueTask.FromResult(result);
    }
    private void Failed(DbContext? context)
    {
        if (context is not null) _pending.Remove(context.ContextId.InstanceId);
    }
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => Failed(eventData.Context);
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        Failed(eventData.Context);
        return Task.CompletedTask;
    }
    public override void SaveChangesCanceled(DbContextEventData eventData) => Failed(eventData.Context);
    public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        Failed(eventData.Context);
        return Task.CompletedTask;
    }
}

public sealed class AuditTransactionInterceptor(AuditSession session) : DbTransactionInterceptor
{
    private void Commit(Guid id)
    {
        foreach (var batch in session.Batches.Where(x => x.TransactionId == id)) batch.Committed = true;
    }
    private void Rollback(Guid id)
    {
        if (session.Current is null) return;
        session.HadRollback = true;
        session.Batches.RemoveAll(x => x.TransactionId == id);
    }
    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) => Commit(eventData.TransactionId);
    public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Commit(eventData.TransactionId);
        return Task.CompletedTask;
    }
    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) => Rollback(eventData.TransactionId);
    public override Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Rollback(eventData.TransactionId);
        return Task.CompletedTask;
    }
}
