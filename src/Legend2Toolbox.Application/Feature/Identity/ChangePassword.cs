using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Domain.Models;

namespace Legend2Toolbox.Application.Feature.Identity;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest<Result>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("密码不能为空");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少需要6位")
            .NotEqual(x => x.OldPassword).WithMessage("新密码不能与旧密码相同");
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ChangePasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.ChangePasswordAsync(request);
    }
}