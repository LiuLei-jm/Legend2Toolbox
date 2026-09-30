using System.Security.Claims;
using Legend2Toolbox.Domain.Entities.Audit;

namespace Legend2Toolbox.Api.Endpoints.Audit;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var personal = routes.MapGroup("/api/audit").WithTags("Audit").RequireAuthorization();
        personal.MapGet("/", ([AsParameters] AuditFilter filter, HttpContext http,
            ApplicationDbContext db, CancellationToken ct) => GetPageAsync(filter, http, db, false, ct));
        personal.MapGet("/{id:guid}", (Guid id, HttpContext http,
            ApplicationDbContext db, CancellationToken ct) => GetDetailAsync(id, http, db, false, ct))
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));

        var admin = routes.MapGroup("/api/admin/audit").WithTags("Audit Administration")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));
        admin.MapGet("/", ([AsParameters] AuditFilter filter, HttpContext http,
            ApplicationDbContext db, CancellationToken ct) => GetPageAsync(filter, http, db, true, ct));
        admin.MapGet("/{id:guid}", (Guid id, HttpContext http,
            ApplicationDbContext db, CancellationToken ct) => GetDetailAsync(id, http, db, true, ct));
        return routes;
    }

    private static IQueryable<AuditLog> VisibleLogs(ApplicationDbContext db, Guid userId, bool all)
    {
        var query = db.AuditLogs.AsNoTracking();
        // A target of somebody else's edit is not the operator. Only login attempts also
        // belong to their identified account, without claiming the caller authenticated.
        return all ? query : query.Where(x => x.ActorUserId == userId ||
            (x.Module == "Auth" && x.Action == "Login" && x.SubjectUserId == userId));
    }

    private static async Task<IResult> GetPageAsync(AuditFilter filter, HttpContext http,
        ApplicationDbContext db, bool all, CancellationToken ct)
    {
        if (!Guid.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Results.Unauthorized();
        if (all && !http.User.IsInRole("SuperAdmin")) return Results.Forbid();
        var page = filter.PageNumber ?? 1;
        var size = filter.PageSize ?? 20;
        if (page < 1 || size is < 1 or > 100 || (long)(page - 1) * size > int.MaxValue ||
            (filter.From.HasValue && filter.To.HasValue && filter.From > filter.To))
            return Results.BadRequest(new { message = "分页或时间范围无效" });

        var query = VisibleLogs(db, userId, all);
        if (filter.From.HasValue)
        {
            var from = filter.From.Value.ToUnixTimeMilliseconds();
            query = query.Where(x => x.OccurredAtUnixMs >= from);
        }
        if (filter.To.HasValue)
        {
            var to = filter.To.Value.ToUnixTimeMilliseconds();
            query = query.Where(x => x.OccurredAtUnixMs <= to);
        }
        if (!string.IsNullOrWhiteSpace(filter.Module)) query = query.Where(x => x.Module == filter.Module);
        if (!string.IsNullOrWhiteSpace(filter.Action)) query = query.Where(x => x.Action == filter.Action);
        if (!string.IsNullOrWhiteSpace(filter.Outcome)) query = query.Where(x => x.Outcome == filter.Outcome);
        if (!string.IsNullOrWhiteSpace(filter.ClientIp)) query = query.Where(x => x.ClientIp == filter.ClientIp);
        if (!string.IsNullOrWhiteSpace(filter.TargetId)) query = query.Where(x => x.TargetId == filter.TargetId);
        // Filters can narrow the authorized set, never replace it.
        if (filter.UserId.HasValue)
            query = query.Where(x => x.ActorUserId == filter.UserId ||
                (x.Module == "Auth" && x.Action == "Login" && x.SubjectUserId == filter.UserId));
        if (!string.IsNullOrWhiteSpace(filter.Account))
        {
            var account = filter.Account.Trim();
            if (Guid.TryParse(account, out var accountId))
                query = query.Where(x => x.ActorUserId == accountId ||
                    (x.Module == "Auth" && x.Action == "Login" && x.SubjectUserId == accountId));
            else
            {
                var normalized = account.ToUpperInvariant();
                var matchingUsers = db.Users.IgnoreQueryFilters()
                    .Where(x => x.NormalizedUserName == normalized).Select(x => x.Id);
                // Match both the current account and historical name snapshots, even after deletion.
                query = query.Where(x =>
                    (x.ActorUserId.HasValue && matchingUsers.Contains(x.ActorUserId.Value)) ||
                    (x.ActorUserName != null && x.ActorUserName.ToUpper() == normalized) ||
                    (x.Module == "Auth" && x.Action == "Login" &&
                        ((x.SubjectUserId.HasValue && matchingUsers.Contains(x.SubjectUserId.Value)) ||
                         (x.TargetName != null && x.TargetName.ToUpper() == normalized))));
            }
        }
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.OccurredAtUnixMs).ThenByDescending(x => x.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return Results.Ok(new PagedResult<AuditLog>(items, page, size, total));
    }

    private static async Task<IResult> GetDetailAsync(Guid id, HttpContext http,
        ApplicationDbContext db, bool all, CancellationToken ct)
    {
        if (!Guid.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Results.Unauthorized();
        if (!http.User.IsInRole("SuperAdmin")) return Results.Forbid();
        var log = await VisibleLogs(db, userId, all).Include(x => x.Details.OrderBy(d => d.Sequence))
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return log is null ? Results.NotFound() : Results.Ok(log);
    }
}

public sealed class AuditFilter
{
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public string? Module { get; init; }
    public string? Action { get; init; }
    public string? Outcome { get; init; }
    public string? ClientIp { get; init; }
    public string? TargetId { get; init; }
    public Guid? UserId { get; init; }
    public string? Account { get; init; }
}
