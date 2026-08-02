namespace Legend2Toolbox.Application.Feature.CardNumber;

public record GetConnectionClientsQuery : IRequest<Result<IEnumerable<ConnectionInfo>>>;

public class GetConnectionClientsQueryHandler : IRequestHandler<GetConnectionClientsQuery, Result<IEnumerable<ConnectionInfo>>>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetConnectionClientsQueryHandler(IConnectionManager connectionManager, IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _connectionManager = connectionManager;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<ConnectionInfo>>> Handle(GetConnectionClientsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<IEnumerable<ConnectionInfo>>.Failure(ErrorMessages.Auth.InvalidUserId);
        var securityKeyStr = await _context.SecurityKeys.AsNoTracking().Where(s => s.UserId == currentUserId).Select(s =>s.Key).FirstOrDefaultAsync( cancellationToken);
        if (string.IsNullOrEmpty(securityKeyStr)) return Result<IEnumerable<ConnectionInfo>>.Failure(ErrorMessages.SeKey.NotFoundValidKey);
        var connections = _connectionManager.GetConnection(securityKeyStr);
        return Result<IEnumerable<ConnectionInfo>>.Success(connections);
    }
}