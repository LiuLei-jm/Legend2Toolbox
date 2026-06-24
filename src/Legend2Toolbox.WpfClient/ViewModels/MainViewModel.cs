using CommunityToolkit.Mvvm.ComponentModel;
using Legend2Toolbox.WpfClient.ViewModels.Pages;

namespace Legend2Toolbox.WpfClient.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LogViewModel _logViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    [ObservableProperty]
    private object _currentView;
    [ObservableProperty]
    private bool _isLogSelected;
    partial void OnIsLogSelectedChanged(bool value)
    {
        if (value) CurrentView = _logViewModel;
    }
    [ObservableProperty]
    private bool _isSettingsSelected;
    partial void OnIsSettingsSelectedChanged(bool value)
    {
        if (value) CurrentView = _settingsViewModel;
    }

    public MainViewModel(LogViewModel logViewModel, SettingsViewModel settingsViewModel)
    {
        _logViewModel = logViewModel ?? throw new ArgumentException(null, nameof(logViewModel));
        _settingsViewModel = settingsViewModel ?? throw new ArgumentException(null, nameof(settingsViewModel));

        _settingsViewModel?.LoadInitialDataCommand.ExecuteAsync(null);

        CurrentView = _logViewModel;
    }

}
