using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Application.Common.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Legend2Toolbox.Infrastructure.SignalR;

public class SignalRClientService : ISignalRClientService, IAsyncDisposable, IDisposable
{
    private HubConnection? _hubConnection;
    private readonly IClientFileOperationService _fileService;
    private readonly IAppLogger<SignalRClientService> _logger;
    private readonly List<IDisposable> _hubMethodSubscriptions = new();
    private CancellationTokenSource? _cts;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _isDisposed;

    public SignalRClientService(IClientFileOperationService fileService, IAppLogger<SignalRClientService> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async Task StartAsync(ConnectionConfig config, CancellationToken token)
    {
        await _lock.WaitAsync(token).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            _ = ConnectInLoopAsync(config, _cts.Token);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task StopAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            await StopInternalAsync().ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task ConnectInLoopAsync(ConnectionConfig config, CancellationToken token)
    {
        int retryDelayMs = 30000;
        while (!token.IsCancellationRequested)
        {
            await StopInternalAsync().ConfigureAwait(false);
            var connectionTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var ctsRegistration = token.Register(() => connectionTcs.TrySetResult(null));
            try
            {
                string urlWithKey = $"{config.ServerUrl}/filePushHub?apiKey={Uri.EscapeDataString(config.ApiKey)}&deviceName={config.DeviceName}";
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(urlWithKey)
                    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                    .Build();
                _hubConnection.Closed += (error) =>
                {
                    if (!token.IsCancellationRequested)
                        _logger.LogDebug($"与服务器连接断开: {error?.Message}");
                    connectionTcs.TrySetResult(null);
                    return Task.CompletedTask;
                };
                _hubConnection.Reconnecting += (error) =>
                {
                    _logger.LogDebug($"网络波动，正在尝试自动恢复连接...原因：{error?.Message}");
                    return Task.CompletedTask;
                };
                _hubConnection.Reconnected += (connectionId) =>
                {
                    _logger.LogDebug($"网络已恢复，自动重连成功！connectionId: {connectionId}");
                    return Task.CompletedTask;
                };
                RegisterHubSubscriptions();
                _logger.LogInfo("正在尝试连接服务器...");
                await _hubConnection.StartAsync(token).ConfigureAwait(false);
                _logger.LogInfo("成功连接服务器!等待指令...");
                await connectionTcs.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInfo("连接过程已取消...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError("连接失败，准备失败...", ex);
            }
            if (!token.IsCancellationRequested)
            {
                _logger.LogDebug($"将在 {retryDelayMs / 1000} 秒后尝试重新连接...");
                try
                {
                    await Task.Delay(retryDelayMs, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInfo("已停止重试.");
                    break;
                }
            }
        }
    }

    private void RegisterHubSubscriptions()
    {
        if (_hubConnection == null) return;
        ClearSubScriptions();
        var subWrite = _hubConnection.On<ModifyContentCommand>("ReceiveWriteCommand", async (cmd) =>
        {
            try
            {
                await _fileService.ModifyFileAppendAsync(cmd).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"执行写入指令失败. File: {cmd.FilePath}", ex);
            }
        });
        _hubMethodSubscriptions.Add(subWrite);
        var subDelete = _hubConnection.On<ModifyContentCommand>("ReceiveDeleteCommand", async (cmd) =>
        {
            try
            {
                await _fileService.RemoveContentFromFileAsync(cmd).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"执行删除指令失败. File: {cmd.FilePath}", ex);
            }
        });
        _hubMethodSubscriptions.Add(subDelete);
    }

    private async Task StopInternalAsync()
    {
        ClearSubScriptions();
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"停止 SignalR 连接时发生错误: {ex.Message}", ex);
            }
            finally
            {
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
                _hubConnection = null;
            }
        }
    }
    private void ClearSubScriptions()
    {
        foreach (var disposable in _hubMethodSubscriptions)
        {
            disposable.Dispose();
        }
        _hubMethodSubscriptions.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(SignalRClientService));
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        await StopInternalAsync().ConfigureAwait(false);
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        Task.Run(async () => await StopInternalAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
