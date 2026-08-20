namespace Legend2Toolbox.WpfClient.ViewModels.Pages;

public partial class LogViewModel : ObservableRecipient, IRecipient<AppLogMessage>
{
    private const int MaxLogsCount = 1_000;
    private const int CleanupCount = 100;
    private readonly Dispatcher _dispatcher;

    public LogViewModel(Dispatcher dispatcher, IMessenger messenger) : base(messenger)
    {
        _dispatcher = dispatcher;
        Logs.CollectionChanged += OnLogsCollectionChanged;
        IsActive = true;
    }

    public ObservableCollection<string> Logs { get; } = [];

    public void Receive(AppLogMessage message)
    {
        var formatted = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff}] {message.Message}";
        _dispatcher.InvokeAsync(() =>
        {
            Logs.Add(formatted);
            if (Logs.Count > MaxLogsCount)
                for (var i = 0; i < CleanupCount; i++)
                    Logs.RemoveAt(0);
        }, DispatcherPriority.Background);
    }

    private void OnLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ClearLogsCommand.NotifyCanExecuteChanged();
    }

    private bool CanClear()
    {
        return Logs.Count > 0;
    }

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