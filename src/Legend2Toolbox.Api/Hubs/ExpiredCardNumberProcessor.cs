namespace Legend2Toolbox.Api.Hubs;

public class ExpiredCardNumberProcessor : BackgroundService
{
    private readonly ILogger<ExpiredCardNumberProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExpiredCardNumberProcessor(ILogger<ExpiredCardNumberProcessor> logger, IServiceProvider servicePorvider)
    {
        _logger = logger;
        _serviceProvider = servicePorvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("卡号过期处理服务已启动......");
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessExpiredCardsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "卡号过期处理服务发生严重异常崩溃.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("服务收到停止信号,准备退出......");
        }
    }

    private async Task ProcessExpiredCardsAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("开始检查过期卡号......");
        using var scope = _serviceProvider.CreateScope();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ResourceSyncHub>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var connectionManager = scope.ServiceProvider.GetRequiredService<IConnectionManager>();
        var utcNow = DateTimeOffset.UtcNow;
        var thresholdTime = utcNow.AddMinutes(-30);

        var expiredCards = await context.CardNumbers.Where(c => c.EndTime < utcNow
        && !c.IsExpiredNotificationSent
        && (c.LastCheckedForConnection == null || c.LastCheckedForConnection <= thresholdTime))
            .Select(c => new { c.Id, c.UserId, c.Cdk })
            .ToListAsync(stoppingToken);

        if (!expiredCards.Any()) return;

        var expiredCardsDict = expiredCards
            .GroupBy(e => e.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => new { x.Id, x.Cdk }).ToList());

        var userIds = expiredCardsDict.Keys.ToList();
        var userSettings = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new
            {
                UserId = u.Id,
                SecurityKey = u.SecurityKey != null ? u.SecurityKey.Key : null,
                CardPath = u.CardNumberPath != null ? u.CardNumberPath.FullPath : null
            }).ToDictionaryAsync(u => u.UserId, stoppingToken);

        foreach (var userGroup in expiredCardsDict)
        {
            var userId = userGroup.Key;
            var cards = userGroup.Value;
            var cardIds = cards.Select(c => c.Id).ToList();

            if (!userSettings.TryGetValue(userId, out var setting) || setting.SecurityKey is null)
            {
                _logger.LogWarning("用户ID {UserId} 没有对应的 SecurityKey", userId);
                continue;
            }
            if (setting.CardPath is null)
            {
                _logger.LogWarning("用户ID {UserId} 没有配置 CardNumberPath", userId);
                continue;
            }
            var connectionIds = connectionManager.GetConnection(setting.SecurityKey)
                .Select(c => c.ConnectionId).ToList();

            if (!connectionIds.Any())
            {
                _logger.LogDebug("用户ID {UserId} 当前不在线，仅更新加检查时间.", userId);
                await context.CardNumbers
                    .Where(c => cardIds.Contains(c.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastCheckedForConnection, utcNow), stoppingToken);
                continue;
            }

            var deleteCdks = cards.Select(v => v.Cdk).ToList();
            var request = new SendDeleteListRequest(
                setting.CardPath,
                deleteCdks
                );

            await hubContext.Clients.Clients(connectionIds)
                .SendAsync(SignalRInteraction.RemoveList, request, stoppingToken);

            await context.CardNumbers
                .Where(c => cardIds.Contains(c.Id))
                .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastCheckedForConnection, utcNow)
                .SetProperty(x => x.LastModifiedOn, utcNow)
                .SetProperty(x => x.IsExpiredNotificationSent, true), stoppingToken);
            _logger.LogInformation("向 {UserId} 成功推送了 {Count} 张过期卡号.", userId, cards.Count);
        }
    }
}
