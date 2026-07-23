using Legend2Toolbox.Domain.Constants;

namespace Legend2Toolbox.Application.Feature.SecurityKey;

public record GetKeyQuery() : IRequest<Result<SecurityKeyResponse>>;

public class GetKeyQueryHandler : IRequestHandler<GetKeyQuery, Result<SecurityKeyResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public GetKeyQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<Result<SecurityKeyResponse>> Handle(GetKeyQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId)) return Result<SecurityKeyResponse>.Failure(ErrorMessages.Auth.InvalidUserId);
        var key = await _context.SecurityKeys.FirstOrDefaultAsync(k => k.UserId == userId, cancellationToken);
        if (key == null) return Result<SecurityKeyResponse>.Failure(ErrorMessages.SeKey.NotFoundValidKey);
        return Result<SecurityKeyResponse>.Success(new SecurityKeyResponse(
            key.Key, key.CreatedOn, key.LastModifiedOn));
    }
}