using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Legend2Toolbox.WpfClient.Messages;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Legend2Toolbox.WpfClient.ViewModels.Pages;

public partial class LogViewModel : ObservableRecipient, IRecipient<AppLogMessage>
{
    private const int MaxLogsCount = 1_000;
    private const int CleanupCount = 100;
    private readonly Dispatcher _dispatcher;
    public ObservableCollection<string> Logs { get; } = [];

    public LogViewModel(Dispatcher dispatcher, IMessenger messenger) : base(messenger)
    {
        _dispatcher = dispatcher;
        Logs.CollectionChanged += OnLogsCollectionChanged;
        IsActive = true;
    }

    private void OnLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ClearLogsCommand.NotifyCanExecuteChanged();
    }

    public void Receive(AppLogMessage message)
    {
        var prefix = message.IsError ? "[ERROR]" : "[INFO]";
        var formatted = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {prefix} {message.Message}";
        _dispatcher.InvokeAsync(() =>
        {
            Logs.Add(formatted);
            if (Logs.Count > MaxLogsCount)
            {
                for (int i = 0; i < CleanupCount; i++)
                    Logs.RemoveAt(0);
            }
        }, DispatcherPriority.Background);
    }

    private bool CanClear() => Logs.Count > 0;
    [RelayCommand(CanExecute = nameof(CanClear))]
    private void ClearLogs()
    {
        Logs.Clear();
    }
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Logs.CollectionChanged -= OnLogsCollectionChanged;
    }
}
