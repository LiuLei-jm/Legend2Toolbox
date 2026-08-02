namespace Legend2Toolbox.Application.Feature.Admin;

public record GetUserByNameCommand(string UserName) : IRequest<Result<UserDto>>;
public class GetUserByNameCommandValidator : AbstractValidator<GetUserByNameCommand>
{
    public GetUserByNameCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空");
    }
}
public class GetUserByNameCommandHandler : IRequestHandler<GetUserByNameCommand, Result<UserDto>>
{
    private readonly IIdentityService _identityService;

    public GetUserByNameCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<UserDto>> Handle(GetUserByNameCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.GetUserByNameAsync(request.UserName);
    }
}
