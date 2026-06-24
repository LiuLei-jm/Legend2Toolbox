using Legend2Toolbox.Application.Common.Models;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IClientConfigurationService
{
    Task<ConnectionConfig?> LoadConfigAsync();
    Task SaveConfigAsync(ConnectionConfig config);
}
