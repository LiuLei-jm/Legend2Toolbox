namespace Legend2Toolbox.Application.Feature.Admin;

public record ToggleUserLockCommand(string UserId, bool LockUser) : IRequest<Result>;

public class ToggleUserLockCommandValidator : AbstractValidator<ToggleUserLockCommand>
{
    public ToggleUserLockCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
    }
}

public class ToggleUserLockCommandHandler : IRequestHandler<ToggleUserLockCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ToggleUserLockCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ToggleUserLockCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.ToggleUserLockAsync(request);
    }
}