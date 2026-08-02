namespace Legend2Toolbox.Application.Feature.SecurityKey;

public record GenerateKeyCommand() : IRequest<Result<SecurityKeyResponse>>;
public record SecurityKeyResponse(string Key, DateTimeOffset CreatedOn, DateTimeOffset? LastModifiedOn);
public class GenerateKeyCommandHandler : IRequestHandler<GenerateKeyCommand, Result<SecurityKeyResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GenerateKeyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SecurityKeyResponse>> Handle(GenerateKeyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var isValid = Guid.TryParse(userId, out Guid userGuid);
        if (!isValid) return Result<SecurityKeyResponse>.Failure(ErrorMessages.Auth.InvalidUserId);
        var existingKey = await _context.SecurityKeys.FirstOrDefaultAsync(k => k.UserId == userGuid, cancellationToken);
        if (existingKey is null)
        {
            existingKey = Domain.Entities.SecurityKey.Create(userGuid, _currentUserService.UserName);
            await _context.SecurityKeys.AddAsync(existingKey, cancellationToken);
        }
        else
        {
            existingKey.RegenerateKey(_currentUserService.UserName!);
        }
        while (await _context.SecurityKeys.AnyAsync(k => k.Key == existingKey.Key && k.Id != userGuid, cancellationToken: cancellationToken))
        {
            existingKey.RegenerateKey(_currentUserService.UserName!);
        }
        await _context.SaveChangesAsync(cancellationToken);
        var response = new SecurityKeyResponse(existingKey.Key, existingKey.CreatedOn, existingKey.LastModifiedOn);
        return Result<SecurityKeyResponse>.Success(response);
    }
}