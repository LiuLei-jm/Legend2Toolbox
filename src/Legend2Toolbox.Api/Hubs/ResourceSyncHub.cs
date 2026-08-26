using ConnectionInfo = Legend2Toolbox.Domain.Models.ConnectionInfo;

namespace Legend2Toolbox.Api.Hubs;

public class ResourceSyncHub : Hub
{
    private readonly IConnectionManager _connectionManager;
    private readonly IHubContext<ResourceSyncHub> _hubContext;
    private readonly ILogger<ResourceSyncHub> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ResourceSyncHub(ILogger<ResourceSyncHub> logger, IConnectionManager connectionManager,
        IServiceScopeFactory scopeFactory, IHubContext<ResourceSyncHub> hubContext)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var key = httpContext?.Request.Query["Key"].ToString();
        var deviceName = httpContext?.Request.Query["DeviceName"].ToString();

        var ipAddress = string.Empty;
        if (httpContext != null)
        {
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
            {
                ipAddress = forwardedHeader.FirstOrDefault()?.Split(',')[0].Trim();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress;
                if (remoteIp != null)
                {
                    if (remoteIp.IsIPv4MappedToIPv6)
                    {
                        remoteIp = remoteIp.MapToIPv4();
                    }
                    ipAddress = remoteIp.ToString();
                }
            }
        }

        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("客户端未使用 API 密钥连接，连接已终止。连接ID : {ConnectionId}", Context.ConnectionId);
            throw new HubException("MissingConnectionKey");
        }

        Guid userId;
        string? cardNumberPath = null;
        string userName;

        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await context.Users
                .Include(u => u.ConnectionKey)
                .Include(u => u.CardNumberPath)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ConnectionKey!.Key == key);

            if (user is null)
            {
                _logger.LogWarning("没有此用户，连接已终止。连接ID: {ConnectionId}", Context.ConnectionId);
                throw new HubException("InvalidConnectionKey");
            }

            userId = user.Id;
            userName = user.UserName!;
            cardNumberPath = user.CardNumberPath?.FullPath;
        }

        if (string.IsNullOrEmpty(deviceName)) deviceName = Context.ConnectionId;
        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = Context.ConnectionId,
            UserName = userName,
            IpAddress = ipAddress ?? string.Empty,
            DeviceName = deviceName,
            ConnectionAt = DateTimeOffset.UtcNow
        };
        _connectionManager.AddConnection(key, connectionInfo);
        _logger.LogInformation("用户: {UserName} 建立 Hub 连接, 连接ID: {ConnectionId}", userName, Context.ConnectionId);

        await base.OnConnectedAsync();
        if (!string.IsNullOrEmpty(cardNumberPath))
            _ = Task.Run(async () =>
            {
                try
                {
                    await SyncUnexpiredCardNumberSafelyAsync(key, userId, cardNumberPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台同步卡号时发生捕获异常");
                }
            });
    }

    public override Task OnDisconnectedAsync(Exception? ex)
    {
        _logger.LogInformation("客户端断开连接, 连接ID：{ConnectionId}", Context.ConnectionId);
        _connectionManager.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(ex);
    }

    private async Task SyncUnexpiredCardNumberSafelyAsync(string connectionKey, Guid userId, string cardNumberPath)
    {
        try
        {
            var utcNow = DateTimeOffset.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var unexpiredCards = await context.CardNumbers
                .Where(c => c.UserId == userId
                            && c.EndTime > utcNow)
                .OrderBy(c => c.CreatedOn)
                .Select(c => c.Cdk)
                .ToListAsync();

            if (!unexpiredCards.Any()) return;

            var connectionIds = _connectionManager.GetConnection(connectionKey).Select(c => c.ConnectionId).ToList();

            if (!connectionIds.Any()) return;

            var request = new SendSyncUnexpiredCardsListRequest(cardNumberPath,
                unexpiredCards);

            await _hubContext.Clients.Clients(connectionIds).SendAsync(SignalRInteraction.SyncUnexpiredCardsList,
                request, CancellationToken.None);

            _logger.LogInformation("向用户发送了 {Count} 条同步卡号通知.", unexpiredCards.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("发送同步卡号通知时客户端断开连接，任务已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送同步卡号通知时发生错误");
        }
    }
}