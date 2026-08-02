namespace Legend2Toolbox.Application.Feature.CardNumber;

public record UpdateCardNumberCommand(
    Guid CardId,
    string Owner,
    int DurationInDays,
    double FaceValue,
    decimal Amount,
    DateTimeOffset StartTime,
    string? Notes) : IRequest<Result>;

public class UpdateCardNumberCommandValidator : AbstractValidator<UpdateCardNumberCommand>
{
    public UpdateCardNumberCommandValidator()
    {
        RuleFor(x => x.CardId).NotEmpty().WithMessage("卡号ID不能为空");
        RuleFor(x => x.Owner).NotEmpty().WithMessage("所有人不能为空")
            .MaximumLength(100).WithMessage("客户名不能超过100个字符"); ;
        RuleFor(x => x.DurationInDays).GreaterThanOrEqualTo(0).WithMessage("天数不能为负数");
        RuleFor(x => x.FaceValue).InclusiveBetween(0, 9999).WithMessage("面值在0~9999之间");
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("金额不能为负数");
        RuleFor(x => x.Notes).MaximumLength(500).WithMessage("备注不能超过500个字符");
    }
}
public class UpdateCardNumberCommandHandler : IRequestHandler<UpdateCardNumberCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCardNumberCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateCardNumberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            return Result.Failure(ErrorMessages.Auth.InvalidUserId);
        var card = await _context.CardNumbers.FindAsync([request.CardId], cancellationToken);
        if (card is null) return Result.Failure(ErrorMessages.Card.NotFoundCard);
        bool isAdmin = _currentUserService.UserName == AdminInfo.AdminUserName;
        bool isOwner = card.UserId == currentUserId;
        if (!isAdmin && !isOwner)
            return Result.Failure(ErrorMessages.Auth.NoPermissionToOperate);

        card.Update(
            request.Owner,
            request.DurationInDays,
            request.FaceValue,
            request.Amount,
            request.StartTime,
            request.Notes,
            _currentUserService.UserName ?? "System");

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}