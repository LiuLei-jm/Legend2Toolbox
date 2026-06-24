using CommunityToolkit.Mvvm.Messaging;
using Legend2Toolbox.Application;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Infrastructure;
using Legend2Toolbox.WpfClient.Logging;
using Legend2Toolbox.WpfClient.ViewModels;
using Legend2Toolbox.WpfClient.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;
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

                services.AddApplication();
                services.AddInfrastructure();

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

