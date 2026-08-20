namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IConnectionManager
{
    void AddConnection(string connectionKey,ConnectionInfo connectionInfo);
    void RemoveConnection(string connectionId);
    IEnumerable<ConnectionInfo> GetConnection(string connectionKey);
    public int GetConnectionCount(string connectionKey);
    IEnumerable<ConnectionInfo> GetAllConnections();
}