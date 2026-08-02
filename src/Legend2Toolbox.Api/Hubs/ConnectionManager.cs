using ConnectionInfo = Legend2Toolbox.Domain.Models.ConnectionInfo;

namespace Legend2Toolbox.Api.Hubs;

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectionInfo>> _connections = new();
    private readonly ConcurrentDictionary<string, string> _connectionIdToKey = new();


    public void AddConnection(string securityKey, string connectionId, string deviceName, string userName)
    {
        _connectionIdToKey.TryAdd(connectionId, securityKey);

        var userConnections = _connections.GetOrAdd(securityKey, _ => new ConcurrentDictionary<string, ConnectionInfo>());

        userConnections.TryAdd(connectionId, new ConnectionInfo
        {
            ConnectionId = connectionId,
            UserName = userName,
            DeviceName = deviceName,
            ConnectionAt = DateTimeOffset.UtcNow
        });
    }

    public IEnumerable<ConnectionInfo> GetAllConnections()
    {
        return _connections.Values.SelectMany(userDict => userDict.Values);
    }

    public IEnumerable<ConnectionInfo> GetConnection(string securityKey)
    {
        return _connections.TryGetValue(securityKey, out var userConnections)
            ? userConnections.Values
            : Enumerable.Empty<ConnectionInfo>();
    }

    public int GetConnectionCount(string securityKey)
    {
        return _connections.TryGetValue(securityKey, out var userConnections)
         ? userConnections.Count : 0;
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connectionIdToKey.TryRemove(connectionId, out var securityKey))
        {
            if (_connections.TryGetValue(securityKey, out var userConnections))
            {
                userConnections.TryRemove(connectionId, out _);
                if (userConnections.IsEmpty)
                    _connections.TryRemove(securityKey, out _);
            }
        }
    }

}
