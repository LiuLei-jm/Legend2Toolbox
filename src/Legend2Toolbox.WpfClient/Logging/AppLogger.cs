
namespace Legend2Toolbox.WpfClient.Logging;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly IMessenger _messenger;

    public AppLogger(ILogger<T> logger, IMessenger messenger)
    {
        _logger = logger;
        _messenger = messenger;
    }

    public void LogError(string message, Exception? ex = null)
    {
        _logger.LogError(ex, message);
        _messenger.Send(new AppLogMessage(message, isError: true));
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
        _messenger.Send(new AppLogMessage(message));
    }

    public void LogDebug(string message, Exception? ex = null)
    {
        _logger.LogDebug(message, ex);
    }
}
