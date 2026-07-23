namespace Legend2Toolbox.Application.Feature.Admin;

public record RemoveUserCommand(string UserId) : IRequest<Result>;

public class RemoveUserCommandHandler : IRequestHandler<RemoveUserCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RemoveUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> Handle(RemoveUserCommand request, CancellationToken cancellationToken)
    {
        return _identityService.RemoveUserAsync(request);
    }
}