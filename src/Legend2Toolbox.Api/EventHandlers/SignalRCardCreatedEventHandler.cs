namespace Legend2Toolbox.Api.EventHandlers;

public class SignalRCardCreatedEventHandler : INotificationHandler<CardNumberCreatedEvent>
{
    private readonly IHubContext<ResourceSyncHub> _hubContext;
    private readonly IConnectionManager _connectionManager;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SignalRCardCreatedEventHandler> _logger;

    public SignalRCardCreatedEventHandler(
        IHubContext<ResourceSyncHub> hubContext,
        IConnectionManager connectionManager,
        IApplicationDbContext context,
        ILogger<SignalRCardCreatedEventHandler> logger)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(CardNumberCreatedEvent notification, CancellationToken cancellationToken)
    {
        var securityKeyEntity = await _context.SecurityKeys.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == notification.UserId, cancellationToken);
        if (securityKeyEntity is null)
        {
            _logger.LogWarning("无法向用户 {UserName} 推送卡号创建命令，原因：未找到该用户的 SecurityKey.", notification.UserName);
            return;
        }
        var cardNumberPathEntity = await _context.CardNumberPaths.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == notification.UserId, cancellationToken);
        if (cardNumberPathEntity is null || string.IsNullOrEmpty(cardNumberPathEntity.FullPath))
        {
            _logger.LogWarning("用户 {UserName} 没有配置有效的卡号同步路径 (CardNumberPath), 跳过 SignalR 推送.", notification.UserName);
            return;
        }
        var request = new SendAppendRequest(
            cardNumberPathEntity.FullPath,
            notification.Cdk,
            $"新增卡号 {notification.Cdk}"
            );

        var connections = _connectionManager.GetConnection(securityKeyEntity.Key).ToList();
        var connectionIds = connections.Select(c => c.ConnectionId).ToList();
        if (connectionIds.Any())
        {
            await _hubContext.Clients.Clients(connectionIds)
                .SendAsync(SignalRInteraction.Append, request, cancellationToken);
            _logger.LogInformation("已向用户 {UserName} 的 {Count} 个在线设备下发创建卡号 {cdk} 命令。", notification.UserName, connectionIds.Count, notification.Cdk);
        }
        else
        {
            _logger.LogDebug("用户 {UserName} 当前无设备在线， 卡号 {Cdk} 的创建命令被丢弃.", notification.UserName, notification.Cdk);
        }
    }
}
