using System.Diagnostics;
using Legend2Toolbox.Domain.Entities.Audit;
using Microsoft.Extensions.Options;

namespace Legend2Toolbox.Infrastructure.Auditing;

public sealed class AuditService(AuditSession session, IHttpContextAccessor httpContextAccessor,
    IOptions<AuditOptions> options, AuditStore store, ILogger<AuditService> logger) : IAuditService
{
    public async Task<T> ExecuteAsync<T>(AuditOperation operation, Func<Task<T>> action)
    {
        // Nested work belongs to the originating user operation.
        if (session.Current is not null) return await action();
        var http = httpContextAccessor.HttpContext;
        var (clientIp, peerIp) = AuditIpResolver.Resolve(http, options.Value);
        var anonymousOperation = operation.ActorType is not null;
        var userId = http?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var log = new AuditLog
        {
            TraceId = Activity.Current?.TraceId.ToString() ?? http?.TraceIdentifier ?? Guid.NewGuid().ToString(),
            ActorUserId = !anonymousOperation && Guid.TryParse(userId, out var id) ? id : null,
            ActorUserName = anonymousOperation ? null : Limit(http?.User.FindFirstValue(ClaimTypes.Name), 256),
            ActorType = operation.ActorType ?? (http?.User.Identity?.IsAuthenticated == true ? "User" : "System"),
            Module = operation.Module,
            Action = operation.Action,
            TargetId = Limit(operation.TargetId, 128),
            TargetName = Limit(operation.TargetName, 256),
            ClientIp = clientIp,
            PeerIp = peerIp
        };
        session.Current = log;
        try
        {
            var response = await action();
            var succeeded = response is not Result result || result.IsSuccess;
            log.Outcome = succeeded ? "Succeeded" : "Failed";
            if (!succeeded) log.ErrorCode = "BusinessRejected";
            if (succeeded && log.SubjectUserId.HasValue &&
                operation.ActorType is "LoginAttempt" or "Registration" or "PasswordReset")
            {
                log.ActorUserId = log.SubjectUserId;
                log.ActorUserName = log.TargetName;
                log.ActorType = "User";
            }
            return response;
        }
        catch (Exception ex)
        {
            log.Outcome = ex is OperationCanceledException ? "Cancelled" : "Failed";
            // Never persist exception messages or request/response bodies containing credentials.
            log.ErrorCode = Limit(ex.GetType().Name, 128);
            throw;
        }
        finally
        {
            try
            {
                var committed = session.Batches.Where(x => x.Committed).ToList();
                log.Details = committed.SelectMany(x => x.Details).ToList();
                for (var i = 0; i < log.Details.Count; i++)
                {
                    log.Details[i].AuditLogId = log.Id;
                    log.Details[i].Sequence = i + 1;
                }
                log.TargetId ??= log.Details.FirstOrDefault()?.EntityId;
                var rolledBack = session.HadRollback || session.Batches.Any(x => !x.Committed);
                log.DataStatus = log.Details.Count > 0
                    ? rolledBack ? "PartiallyCommitted" : "Committed"
                    : rolledBack ? "RolledBack" : "NoChanges";
                if (log.CaptureIncomplete) log.DataStatus = "Unknown";
                // A disconnected HTTP client must not cancel persistence of committed work.
                await store.WriteAsync(log);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "审计记录 {AuditId} 持久化失败，原业务结果保持不变", log.Id);
            }
            finally
            {
                session.Reset();
            }
        }
    }

    public void IdentifySubject(Guid userId, string? userName)
    {
        if (session.Current is not { } log) return;
        log.SubjectUserId = userId;
        log.TargetId = userId.ToString();
        log.TargetName = Limit(userName, 256);
    }

    private static string? Limit(string? value, int length) => value?.Length > length ? value[..length] : value;
}
