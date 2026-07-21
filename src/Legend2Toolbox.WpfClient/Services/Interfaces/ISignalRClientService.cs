namespace Legend2Toolbox.WpfClient.Services.Interfaces;

public interface ISignalRClientService
{
    Task StartAsync(ConnectionConfig config, CancellationToken token);
    Task StopAsync();
}
