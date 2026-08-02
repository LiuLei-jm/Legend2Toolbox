namespace Legend2Toolbox.Application.Feature.Admin;

public record GetAllConnectionClientsQuery : IRequest<Result<IEnumerable<ConnectionInfo>>>;

public class GetAllConnectionClientsQueryHandler : IRequestHandler<GetAllConnectionClientsQuery, Result<IEnumerable<ConnectionInfo>>>
{
    private readonly IConnectionManager _connectionManager;

    public GetAllConnectionClientsQueryHandler(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<Result<IEnumerable<ConnectionInfo>>> Handle(GetAllConnectionClientsQuery request, CancellationToken cancellationToken)
    {
        var allConnections = _connectionManager.GetAllConnections();
        return Result<IEnumerable<ConnectionInfo>>.Success(allConnections);
    }
}