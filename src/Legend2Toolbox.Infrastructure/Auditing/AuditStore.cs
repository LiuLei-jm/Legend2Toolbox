using System.Text.Json;
using Legend2Toolbox.Domain.Entities.Audit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Legend2Toolbox.Infrastructure.Auditing;

public sealed class AuditStore(IServiceScopeFactory scopeFactory, IOptions<AuditOptions> options,
    ILogger<AuditStore> logger)
{
    public async Task WriteAsync(AuditLog log)
    {
        string? pendingFile = null;
        try
        {
            Directory.CreateDirectory(options.Value.SpoolDirectory);
            pendingFile = Path.Combine(options.Value.SpoolDirectory, log.Id.ToString("N") + ".json");
            var temporaryFile = pendingFile + ".tmp";
            await using (var stream = new FileStream(temporaryFile, FileMode.Create, FileAccess.Write,
                             FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, log);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporaryFile, pendingFile, overwrite: true);
        }
        catch (Exception ex)
        {
            pendingFile = null;
            logger.LogCritical(ex, "审计 {AuditId} 无法写入持久化补写目录，将尝试直接写库", log.Id);
        }
        try
        {
            await PersistAsync(log);
            if (pendingFile is not null) File.Delete(pendingFile);
        }
        catch (Exception ex)
        {
            if (pendingFile is null)
                logger.LogCritical(ex, "审计 {AuditId} 写库和持久化暂存均失败，需要立即处理", log.Id);
            else
                logger.LogError(ex, "审计 {AuditId} 写库失败，记录已持久化等待补写", log.Id);
        }
    }

    private async Task PersistAsync(AuditLog log)
    {
        var seconds = Math.Clamp(options.Value.WriteTimeoutSeconds, 1, 30);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.SetCommandTimeout(seconds);
        if (await context.AuditLogs.AnyAsync(x => x.Id == log.Id, timeout.Token)) return;
        context.AuditLogs.Add(log);
        await context.SaveChangesAsync(timeout.Token);
    }

    public async Task RetryPendingAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(options.Value.SpoolDirectory)) return;
        foreach (var file in Directory.EnumerateFiles(options.Value.SpoolDirectory, "*.json").Take(100))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var log = JsonSerializer.Deserialize<AuditLog>(json)
                    ?? throw new InvalidDataException("审计补写记录为空");
                await PersistAsync(log);
                File.Delete(file);
            }
            catch (FileNotFoundException) { }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "审计补写失败，保留文件 {AuditFile}", Path.GetFileName(file));
            }
        }
    }
}

public sealed class AuditRetryService(AuditStore store, ILogger<AuditRetryService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            do
            {
                try { await store.RetryPendingAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { logger.LogError(ex, "审计补写服务本轮执行失败"); }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
