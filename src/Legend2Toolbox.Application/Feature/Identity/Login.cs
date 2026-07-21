namespace Legend2Toolbox.Application.Feature.Identity;

public record LoginCommand(string Username, string Password) : IRequest<Result<ClaimsPrincipal>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(x => x.Password).NotEmpty().WithMessage("密码不能为空");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<ClaimsPrincipal>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<ClaimsPrincipal>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AuthenticateUserAsync(request);
    }
}
