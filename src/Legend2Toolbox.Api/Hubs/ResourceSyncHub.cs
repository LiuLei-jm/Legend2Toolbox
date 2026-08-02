using System.Net.WebSockets;

namespace Legend2Toolbox.Api.Hubs;

public class ResourceSyncHub : Hub
{
    private readonly ILogger<ResourceSyncHub> _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly IServiceScopeFactory _scopeFactory;

    public ResourceSyncHub(ILogger<ResourceSyncHub> logger, IConnectionManager connectionManager, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _scopeFactory = scopeFactory;
    }

    public override async Task OnConnectedAsync()
    {
        var securityKey = Context.GetHttpContext()?.Request.Query["Key"].ToString();
        var deviceName = Context.GetHttpContext()?.Request.Query["DeviceName"].ToString();

        if (string.IsNullOrEmpty(securityKey))
        {
            _logger.LogWarning("客户端未使用 API 密钥连接，连接已终止。连接ID : {ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await context.Users
            .Include(u => u.SecurityKey)
            .Include(u => u.CardNumberPath)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.SecurityKey!.Key == securityKey);
        if (user is null)
        {
            _logger.LogWarning("没有此用户，连接已终止。连接ID: {ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }
        if (string.IsNullOrEmpty(deviceName)) deviceName = Context.ConnectionId;
        _connectionManager.AddConnection(securityKey, Context.ConnectionId, deviceName, user.UserName!);
        _logger.LogInformation("用户: {UserName} 建立 Hub 连接, 连接ID: {ConnectionId}", user.UserName, Context.ConnectionId);

        await base.OnConnectedAsync();
        if (user.CardNumberPath != null && !string.IsNullOrEmpty(user.CardNumberPath.FullPath))
            _ = SendPendingExpiredCardNumber(securityKey, user.Id, user.CardNumberPath.FullPath, Context.ConnectionAborted);
    }

    public override Task OnDisconnectedAsync(Exception? ex)
    {
        _logger.LogInformation("客户端断开连接, 连接ID：{ConnectionId}", Context.ConnectionId);
        _connectionManager.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(ex);
    }

    private async Task SendPendingExpiredCardNumber(string securityKey, Guid userId, string cardNumberPath, CancellationToken cancellationToken)
    {
        try
        {
            var utcNow = DateTimeOffset.UtcNow;
            var thirtyDaysAgo = utcNow.AddDays(-30);
            var oneMinuteAgo = utcNow.AddMinutes(-1);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingCards = await context.CardNumbers
                .Where(c => c.UserId == userId &&
                c.EndTime <= utcNow &&
                c.EndTime >= thirtyDaysAgo &&
                (c.LastCheckedForConnection == null || c.LastCheckedForConnection <= oneMinuteAgo))
                .OrderBy(c => c.EndTime)
                .Take(20)
                .Select(c => new { c.Id, c.Cdk })
                .ToListAsync(cancellationToken);
            if (!pendingCards.Any()) return;

            var connections = _connectionManager.GetConnection(securityKey).ToList();
            var connectionIds = connections.Select(c => c.ConnectionId).ToList();

            if (!connectionIds.Any()) return;

            var tasks = pendingCards.Select(card =>
            {
                var request =
                new SendDeleteRequest(
                    cardNumberPath,
                    card.Cdk,
                    $"卡号 {card.Cdk} 已过期");
                return Clients.Clients(connectionIds).SendAsync("ReceiveDeleteCommand", request, cancellationToken);
            });

            await Task.WhenAll(tasks);

            var cardIds = pendingCards.Select(c => c.Id).ToList();
            await context.CardNumbers
                .Where(c => cardIds.Contains(c.Id))
                .ExecuteUpdateAsync(s =>
                s.SetProperty(c => c.IsExpiredNotificationSent, true)
                .SetProperty(c => c.LastModifiedOn, utcNow)
                .SetProperty(c => c.LastCheckedForConnection, utcNow),
                cancellationToken);

            _logger.LogInformation("向用户发送了 {Count} 条过期卡号通知并更新数据库.", pendingCards.Count);

        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("发送过期开号通知时客户端断开连接，任务已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送过期卡号通知时发生错误");
        }
    }
}
