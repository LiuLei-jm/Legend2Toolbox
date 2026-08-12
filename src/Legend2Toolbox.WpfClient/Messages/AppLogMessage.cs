namespace Legend2Toolbox.WpfClient.Messages;

public class AppLogMessage
{
    public AppLogMessage(string message, bool isError = false)
    {
        Message = message;
        IsError = isError;
    }

    public string Message { get; }
    public bool IsError { get; }
}