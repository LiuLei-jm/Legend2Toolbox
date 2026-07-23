using Legend2Toolbox.Shared.Helpers;

namespace Legend2Toolbox.Application.Feature.CardNumber;

public record CreateCardNumberCommand(
    string CustomerName,
    int DurationInDays,
    double FaceValue,
    decimal Amount,
    string? Cdk,
    string? Notes) : IRequest<Result<Guid>>;

public class CreateCardNumberCommandValidator : AbstractValidator<CreateCardNumberCommand>
{
    public CreateCardNumberCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("客户名不能为空")
            .MaximumLength(100).WithMessage("会员卡名称不能超过100个字符");
        RuleFor(x => x.DurationInDays)
            .GreaterThan(0).WithMessage("卡号持续时间必须大于 0 天");
        RuleFor(x => x.FaceValue)
            .GreaterThanOrEqualTo(0).WithMessage("金额面值不能为负数");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("金额不能为负数");
    }
}

public class CreateCardNumberCommandHandler : IRequestHandler<CreateCardNumberCommand, Result<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateCardNumberCommandHandler> _logger;

    public CreateCardNumberCommandHandler(ICurrentUserService currentUserService, IApplicationDbContext context, ILogger<CreateCardNumberCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCardNumberCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? "System";
        var userName = _currentUserService.UserName ?? "System";
        var cdk = string.IsNullOrWhiteSpace(request.Cdk) ? CdkHelper.GenerateMembershipCard(20, request.FaceValue) : request.Cdk;
        var cardNumber = Domain.Entities.CardNumber.Create(request.CustomerName,
            request.DurationInDays, request.FaceValue, request.Amount, cdk, userId, userName);
        await _context.CardNumbers.AddAsync(cardNumber, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(cardNumber.Id);
    }
}