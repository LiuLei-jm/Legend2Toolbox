namespace Legend2Toolbox.Application.Feature.Admin;

public record AssignRoleCommand(string UserId, string RoleName) : IRequest<Result>;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        var validRoles = string.Join(", ", Enum.GetNames<Roles>());
        RuleFor(x => x.RoleName).NotEmpty()
            .WithMessage("角色名不能为空")
            .IsEnumName(typeof(Roles), caseSensitive: false)
            .WithName($"无效的角色名。可选范围：[{validRoles}]");
    }
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IIdentityService _identityService;

    public AssignRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AssignRoleAsync(request);
    }
}