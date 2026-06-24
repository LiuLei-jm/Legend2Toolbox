using Legend2Toolbox.Application.Common.Models;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface ISignalRClientService
{
    Task StartAsync(ConnectionConfig config, CancellationToken token);
    Task StopAsync();
}
