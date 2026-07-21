namespace Legend2Toolbox.WpfClient.Services.Interfaces;

public interface IClientConfigurationService
{
    Task<ConnectionConfig?> LoadConfigAsync();
    Task SaveConfigAsync(ConnectionConfig config);
}
