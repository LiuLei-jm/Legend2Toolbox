namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IConnectionManager
{
    void AddConnection(string securityKey, string connectionId, string deviceName, string userName);
    void RemoveConnection(string connectionId);
    IEnumerable<ConnectionInfo> GetConnection(string securityKey);
    public int GetConnectionCount(string securityKey);
    IEnumerable<ConnectionInfo> GetAllConnections();
}
