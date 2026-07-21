namespace Legend2Toolbox.WpfClient.Services.Interfaces;

public interface IAppLogger<T>
{
    void LogInfo(string message);
    void LogError(string message, Exception? ex = null);
    void LogDebug(string message, Exception? ex = null);
}
