namespace Legend2Toolbox.Application.Feature.CardNumber;

public record CleanUpCardCommand(string CardId, string Cdk) : IRequest<Result>;
public class CleanUpCardCommandValidator: AbstractValidator<CleanUpCardCommand>
{
    public CleanUpCardCommandValidator()
    {
        RuleFor(c => c.CardId).NotEmpty().WithMessage("卡号ID不能为空");
        RuleFor(c => c.Cdk).NotEmpty().WithMessage("卡号不能为空");
    }
}
public class CleanUpCardCommandHandler : IRequestHandler<CleanUpCardCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CleanUpCardCommandHandler> _logger;
    private readonly IPublisher _publisher;

    public CleanUpCardCommandHandler(ICurrentUserService currentUserService, ILogger<CleanUpCardCommandHandler> logger, IPublisher publisher)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<Result> Handle(CleanUpCardCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result.Failure(ErrorMessages.AuthError.InvalidUserId);
        if (!Guid.TryParse(request.CardId, out var cardGuid)) return Result.Failure(ErrorMessages.CardError.NotFoundCard);
        _logger.LogInformation("用户: {UserName} 清理卡号 {Cdk} .", _currentUserService.UserName, request.Cdk);
        await _publisher.Publish(new CardNumberDeletedEvent(
            cardGuid,
            currentUserId,
            request.Cdk,
            _currentUserService.UserName ?? "System"
        ), cancellationToken);
        return Result.Success();
    }
}