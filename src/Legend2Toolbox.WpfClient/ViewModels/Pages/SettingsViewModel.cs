using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Application.Common.Models;
using Legend2Toolbox.WpfClient.Messages;

namespace Legend2Toolbox.WpfClient.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IClientConfigurationService _configService;
    private readonly ISignalRClientService _signalRService;
    private CancellationTokenSource? _cts;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string _serverUrl = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string _deviceName = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string _apiKey = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private bool _isInputsEnabled = true;
    [ObservableProperty]
    private string _connectionButtonContent = "连接";
    private bool CanSave() => IsInputsEnabled
        && !string.IsNullOrEmpty(ServerUrl)
        && !string.IsNullOrEmpty(DeviceName)
        && !string.IsNullOrEmpty(ApiKey);

    public SettingsViewModel(IClientConfigurationService configService, ISignalRClientService signalRService)
    {
        _configService = configService;
        _signalRService = signalRService;
    }
    [RelayCommand]
    private async Task LoadInitialDataAsync()
    {
        var config = await _configService.LoadConfigAsync();
        if (config != null)
        {
            ServerUrl = config.ServerUrl;
            ApiKey = config.ApiKey;
            DeviceName = string.IsNullOrEmpty(config.DeviceName) ? "PC-" + Environment.MachineName : config.DeviceName;
            if (!string.IsNullOrEmpty(ServerUrl) && !string.IsNullOrEmpty(ApiKey))
                await ToggleConnectionAsync();
        }
    }
    [RelayCommand]
    private async Task ToggleConnectionAsync()
    {
        if (_cts != null)
        {
            WeakReferenceMessenger.Default.Send(new AppLogMessage("用户请求断开连接..."));
            _cts.Cancel();
            await _signalRService.StopAsync();
            _cts = null;
            ConnectionButtonContent = "连接";
            IsInputsEnabled = true;
        }
        else
        {
            _cts = new CancellationTokenSource();
            ConnectionButtonContent = "断开连接";
            IsInputsEnabled = false;
            var config = new ConnectionConfig
            {
                ServerUrl = ServerUrl,
                DeviceName = DeviceName,
                ApiKey = ApiKey,
            };
            _ = _signalRService.StartAsync(config, _cts.Token);
        }
    }
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveConfigAsync()
    {
        var config = new ConnectionConfig
        {
            ServerUrl = ServerUrl,
            DeviceName = DeviceName,
            ApiKey = ApiKey,
        };
        await _configService.SaveConfigAsync(config);
        WeakReferenceMessenger.Default.Send(new AppLogMessage("配置保存成功"));
        Notification.Show("保存成功!");
    }
}
