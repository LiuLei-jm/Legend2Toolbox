using ApplicationContext = System.Windows.Application;

namespace Legend2Toolbox.WpfClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : ApplicationContext
{
    public static IHost AppHost { get; private set; } = default!;
    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "logs/client-log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30
            )
            .CreateLogger();
        AppHost = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                services.AddSingleton<IClientConfigurationService, ClientConfigurationService>();
                services.AddSingleton<IClientFileOperationService, ClientFileOperationService>();
                services.AddSingleton<ISignalRClientService, SignalRClientService>();

                services.AddTransient<LogViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
                services.AddSingleton(_ => Current.Dispatcher);
            })
            .Build();
    }
    protected override async void OnStartup(StartupEventArgs startupArgs)
    {
        base.OnStartup(startupArgs);
        await AppHost.StartAsync();
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    protected override async void OnExit(ExitEventArgs exitArgs)
    {
        base.OnExit(exitArgs);
        await AppHost.StopAsync();
        AppHost.Dispose();
        Log.CloseAndFlush();
    }
}

