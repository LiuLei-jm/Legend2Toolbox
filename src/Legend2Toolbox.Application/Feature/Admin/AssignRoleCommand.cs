namespace Legend2Toolbox.Application.Feature.Admin;

public record AssignRoleCommand(string UserId, List<string> RoleNames) : IRequest<Result>;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        var validRoles = string.Join(", ", Enum.GetNames<Roles>());
        RuleFor(x => x.RoleNames)
            .NotEmpty().WithMessage("角色列表不能为空");
        RuleForEach(x => x.RoleNames)
            .IsEnumName(typeof(Roles), false)
            .WithMessage($"无效的角色名称。 可选范围：[{validRoles}]");
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