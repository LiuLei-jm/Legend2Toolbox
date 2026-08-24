namespace Legend2Toolbox.Application.Feature.Admin;

public record ToggleUserLockCommand(string UserId, bool LockUser) : IRequest<Result<bool>>;

public class ToggleUserLockCommandValidator : AbstractValidator<ToggleUserLockCommand>
{
    public ToggleUserLockCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
    }
}

public class ToggleUserLockCommandHandler : IRequestHandler<ToggleUserLockCommand, Result<bool>>
{
    private readonly IIdentityService _identityService;

    public ToggleUserLockCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<bool>> Handle(ToggleUserLockCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.ToggleUserLockAsync(request);
    }
}