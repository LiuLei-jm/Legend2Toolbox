using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Legend2Toolbox.Application.Feature.Identity;

public record UpdateUserProfileCommand(string NickName, string Email, string PhoneNumber) : IRequest<Result>;

public class UdpateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UdpateUserProfileCommandValidator()
    {
        RuleFor(x => x.NickName)
            .MaximumLength(128).WithMessage("昵称最大长度不能超过128个字符");
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("请输入有效的电子邮箱地址");
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^1[3-9]\d{9}$").WithMessage("请输入有效的11位手机号码")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IIdentityService _identityService;

    public UpdateUserProfileCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.UpdateUserProfileAsync(request);
    }
}