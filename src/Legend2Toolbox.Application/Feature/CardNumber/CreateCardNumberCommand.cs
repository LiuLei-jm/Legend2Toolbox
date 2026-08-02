
namespace Legend2Toolbox.Application.Feature.CardNumber;

public record CreateCardNumberCommand(string Owner,
                                      int DurationInDays,
                                      double FaceValue,
                                      decimal Amount,
                                      string? Notes) : IRequest<Result<Guid>>;

public class CreateCardNumberCommandValidator : AbstractValidator<CreateCardNumberCommand>
{
    public CreateCardNumberCommandValidator()
    {
        RuleFor(x => x.Owner)
            .NotEmpty().WithMessage("客户名不能为空")
            .MaximumLength(100).WithMessage("客户名称不能超过100个字符");
        RuleFor(x => x.DurationInDays)
            .GreaterThan(0).WithMessage("卡号持续时间必须大于 0 天");
        RuleFor(x => x.FaceValue).InclusiveBetween(0,
                                                   9999)
                                 .WithMessage("面值在0~9999之间");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("金额不能为负数");
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("备注不能超过500个字符");
    }
}

public class CreateCardNumberCommandHandler : IRequestHandler<CreateCardNumberCommand, Result<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateCardNumberCommandHandler> _logger;
    private readonly IPublisher _publisher;

    public CreateCardNumberCommandHandler(ICurrentUserService currentUserService, IApplicationDbContext context, ILogger<CreateCardNumberCommandHandler> logger, IPublisher publisher)
    {
        _currentUserService = currentUserService;
        _context = context;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(CreateCardNumberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return Result<Guid>.Failure(ErrorMessages.Auth.InvalidUserId);
        var userName = _currentUserService.UserName ?? "System";
        var cdk = CdkHelper.GenerateMembershipCard(20, request.FaceValue);
        var cardNumberEntity = Domain.Entities.CardNumber.Create(request.Owner,
                                                                 request.DurationInDays,
                                                                 request.FaceValue,
                                                                 request.Amount,
                                                                 cdk,
                                                                 userId,
                                                                 userName,
                                                                 request.Notes);
        await _context.CardNumbers.AddAsync(cardNumberEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("用户: {UserName} 创建卡号 {Cdk} .", userName, cdk);
        await _publisher.Publish(new CardNumberCreatedEvent(
            cardNumberEntity.Id,
            userId,
            cardNumberEntity.Cdk,
            _currentUserService.UserName ?? "System"
            ), cancellationToken);
        return Result<Guid>.Success(cardNumberEntity.Id);
    }
}