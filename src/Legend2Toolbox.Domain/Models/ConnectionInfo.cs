namespace Legend2Toolbox.Domain.Models;

public class ConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTimeOffset ConnectionAt { get; set; } = DateTimeOffset.UtcNow;
}
