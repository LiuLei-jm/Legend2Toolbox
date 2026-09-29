namespace Legend2Toolbox.Application.Feature.Admin;

public record AdjustMembershipDaysCommand(string UserId, int Days) : IRequest<Result<MembershipStatusDto>>;

public class AdjustMembershipDaysCommandValidator : AbstractValidator<AdjustMembershipDaysCommand>
{
    public AdjustMembershipDaysCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.Days).NotEqual(0).WithMessage("会员时间调整天数不能为0")
            .InclusiveBetween(-3650, 3650).WithMessage("调整范围必须在-3650到3650天之间");
    }
}

public class AdjustMembershipDaysCommandHandler : IRequestHandler<AdjustMembershipDaysCommand, Result<MembershipStatusDto>>
{
    private readonly IMembershipService _membershipService;

    public AdjustMembershipDaysCommandHandler(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    public Task<Result<MembershipStatusDto>> Handle(AdjustMembershipDaysCommand request,
        CancellationToken cancellationToken) =>
        _membershipService.AdjustMembershipDaysAsync(request.UserId, request.Days, cancellationToken);
}
