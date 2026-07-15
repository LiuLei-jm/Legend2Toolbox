using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Domain.Models;
using System.Security.Claims;

namespace Legend2Toolbox.Application.Feature.Identity;

public record LoginQuery(string Username, string Password) : IRequest<Result<ClaimsPrincipal>>;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(x => x.Password).NotEmpty().WithMessage("密码不能为空");
    }
}

public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<ClaimsPrincipal>>
{
    private readonly IIdentityService _identityService;

    public LoginQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<ClaimsPrincipal>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.AuthenticateUserAsync(request);
    }
}
