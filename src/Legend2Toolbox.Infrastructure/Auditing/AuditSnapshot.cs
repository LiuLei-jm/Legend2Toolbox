using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Legend2Toolbox.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Legend2Toolbox.Infrastructure.Auditing;

internal static class AuditSnapshot
{
    private static readonly Dictionary<string, string[]> Fields = new()
    {
        ["ApplicationUser"] = ["Id", "UserName", "NickName", "Email", "PhoneNumber", "IsActive", "IsDeleted", "LockoutEnd", "AccessFailedCount", "LastLoginAt"],
        ["IdentityUserRole`1"] = ["UserId", "RoleId"],
        ["ApplicationRole"] = ["Id", "Name"],
        ["CardNumber"] = ["Id", "UserId", "Owner", "StartTime", "EndTime", "DurationInDays", "FaceValue", "Amount", "Notes", "IsDeleted"],
        ["CardNumberPath"] = ["Id", "UserId", "BasePath", "FileName", "AllowCustomPath"],
        ["ConnectionKey"] = ["Id", "UserId"],
        ["ScriptSet"] = ["Id", "UserId", "Name", "Description"],
        ["ScriptFile"] = ["Id", "ScriptSetId", "FileName", "FilePath", "Type"],
        ["ScriptSegment"] = ["Id", "ScriptFileId", "TriggerField"],
        ["MaterialFile"] = ["Id", "ScriptSetId", "FileName", "TargetPath", "FileSize", "Sha256"],
        ["ScriptSetDbData"] = ["Id", "ScriptSetId", "TableType", "Name"],
        ["UserMembership"] = ["Id", "UserId", "StartTime", "ExpireTime", "Source"],
        ["MembershipPaymentOrder"] = ["Id", "UserId", "OrderId", "Provider", "Amount", "DurationDays", "Status", "TradeNo", "PaidOn"]
    };

    private static readonly HashSet<string> SecretFields = ["Password", "PasswordHash", "Key", "Cdk"];
    private static readonly HashSet<string> ContentFields = ["WholeContent", "Content", "DataJson"];

    public static async Task<List<AuditLogDetail>> CaptureAsync(DbContext context, bool async,
        CancellationToken cancellationToken)
    {
        context.ChangeTracker.DetectChanges();
        var entries = context.ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        var details = new List<AuditLogDetail>();
        var deleted = new HashSet<string>();
        foreach (var entry in entries)
        {
            var detail = CaptureEntry(entry);
            if (detail is not null) details.Add(detail);
            if (entry.State == EntityState.Deleted) deleted.Add(entry.Metadata.Name + ":" + EntityId(entry));
        }
        foreach (var entry in entries.Where(x => x.State == EntityState.Deleted))
            await CaptureCascadeAsync(context, entry.Metadata, entry.Entity, deleted, details, async, cancellationToken);
        return details;
    }

    private static AuditLogDetail? CaptureEntry(EntityEntry entry)
    {
        if (!Fields.TryGetValue(entry.Metadata.ClrType.Name, out var fields)) return null;
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            if (!fields.Contains(name) && !SecretFields.Contains(name) && !ContentFields.Contains(name)) continue;
            if (entry.State == EntityState.Modified &&
                (!property.IsModified || Equals(property.OriginalValue, property.CurrentValue))) continue;
            if (entry.State != EntityState.Added) oldValues[name] = SafeValue(name, property.OriginalValue);
            if (entry.State != EntityState.Deleted) newValues[name] = SafeValue(name, property.CurrentValue);
        }
        if (oldValues.Count == 0 && newValues.Count == 0) return null;
        var changeType = entry.State.ToString();
        if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("IsDeleted") is not null &&
            entry.Property("IsDeleted").OriginalValue is false && entry.Property("IsDeleted").CurrentValue is true)
            changeType = "SoftDeleted";
        return new AuditLogDetail
        {
            EntityType = entry.Metadata.ClrType.Name, EntityId = EntityId(entry), ChangeType = changeType,
            OldValues = JsonSerializer.Serialize(oldValues), NewValues = JsonSerializer.Serialize(newValues)
        };
    }

    private static object? SafeValue(string name, object? value)
    {
        if (SecretFields.Contains(name)) return "[REDACTED]";
        if (ContentFields.Contains(name) && value is string content)
            return new { Length = content.Length, Sha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant() };
        return value is string text && text.Length > 2048 ? text[..2048] + "…[truncated]" : value;
    }

    private static string EntityId(EntityEntry entry) => string.Join("/",
        entry.Metadata.FindPrimaryKey()!.Properties.Select(x => entry.Property(x.Name).CurrentValue));

    private static async Task CaptureCascadeAsync(DbContext context, IEntityType parentType, object parent,
        HashSet<string> deleted, List<AuditLogDetail> details, bool async, CancellationToken cancellationToken)
    {
        foreach (var foreignKey in parentType.GetReferencingForeignKeys()
                     .Where(x => x.DeleteBehavior == DeleteBehavior.Cascade))
        {
            var childType = foreignKey.DeclaringEntityType;
            if (!Fields.ContainsKey(childType.ClrType.Name)) continue;
            var keys = foreignKey.PrincipalKey.Properties.Select(p => p.PropertyInfo!.GetValue(parent)).ToArray();
            var queryMethod = typeof(AuditSnapshot).GetMethod(nameof(LoadChildrenAsync), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(childType.ClrType);
            var children = await (Task<List<object>>)queryMethod.Invoke(null,
                [context, foreignKey.Properties.Select(x => x.Name).ToArray(), keys, async, cancellationToken])!;
            foreach (var child in children)
            {
                var id = string.Join("/", childType.FindPrimaryKey()!.Properties.Select(p => p.PropertyInfo!.GetValue(child)));
                if (!deleted.Add(childType.Name + ":" + id)) continue;
                var values = childType.GetProperties()
                    .Where(p => Fields[childType.ClrType.Name].Contains(p.Name) || ContentFields.Contains(p.Name))
                    .ToDictionary(p => p.Name, p => SafeValue(p.Name, p.PropertyInfo!.GetValue(child)));
                details.Add(new AuditLogDetail
                {
                    EntityType = childType.ClrType.Name, EntityId = id, ChangeType = "CascadeDeleted",
                    OldValues = JsonSerializer.Serialize(values)
                });
                await CaptureCascadeAsync(context, childType, child, deleted, details, async, cancellationToken);
            }
        }
    }

    private static async Task<List<object>> LoadChildrenAsync<T>(DbContext context, string[] fields,
        object?[] values, bool async, CancellationToken cancellationToken) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "entity");
        Expression? predicate = null;
        for (var i = 0; i < fields.Length; i++)
        {
            var member = Expression.Property(parameter, fields[i]);
            var comparison = Expression.Equal(member, Expression.Convert(Expression.Constant(values[i]), member.Type));
            predicate = predicate is null ? comparison : Expression.AndAlso(predicate, comparison);
        }
        var query = context.Set<T>().IgnoreQueryFilters().AsNoTracking()
            .Where(Expression.Lambda<Func<T, bool>>(predicate!, parameter));
        var children = async ? await query.ToListAsync(cancellationToken) : query.ToList();
        return children.Cast<object>().ToList();
    }
}
