namespace Legend2Toolbox.Application.Feature.Identity;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<Result>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名至少需要3个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少需要6位");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("电子邮箱不能为空")
            .EmailAddress().WithMessage("请输入有效的电子邮箱地址");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterUserAsync(request);
    }
}