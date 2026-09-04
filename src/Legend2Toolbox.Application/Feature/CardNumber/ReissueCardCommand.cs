namespace Legend2Toolbox.Application.Feature.CardNumber;

public record ReissueCardCommand(string CardId, string Cdk) : IRequest<Result>;
public class ReissueCardCommandValidator: AbstractValidator<ReissueCardCommand>
{
    public ReissueCardCommandValidator()
    {
        RuleFor(c => c.CardId).NotEmpty().WithMessage("卡号ID不能为空");
        RuleFor(c => c.Cdk).NotEmpty().WithMessage("卡号不能为空");
    }
}
public class ReissueCardCommandHandler : IRequestHandler<ReissueCardCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;
    private readonly ILogger<ReissueCardCommandHandler> _logger;

    public ReissueCardCommandHandler(ICurrentUserService currentUserService, IPublisher publisher, ILogger<ReissueCardCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result> Handle(ReissueCardCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result.Failure(ErrorMessages.AuthError.InvalidUserId);
        if (!Guid.TryParse(request.CardId, out var cardGuid)) return Result.Failure(ErrorMessages.CardError.NotFoundCard);
        _logger.LogInformation("用户: {UserName} 补发卡号 {Cdk} .", _currentUserService.UserName, request.Cdk);
        await _publisher.Publish(new CardNumberCreatedEvent(
            cardGuid,
            currentUserId,
            request.Cdk,
            _currentUserService.UserName ?? "System"
        ), cancellationToken);
        return Result.Success();
    }
}
