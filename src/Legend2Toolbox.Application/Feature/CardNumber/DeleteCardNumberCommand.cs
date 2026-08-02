namespace Legend2Toolbox.Application.Feature.CardNumber;

public record DeleteCardNumberCommand(Guid CardId) : IRequest<Result>;

public class DeleteCardNumberCommandValidator : AbstractValidator<DeleteCardNumberCommand>
{
    public DeleteCardNumberCommandValidator()
    {
        RuleFor(x => x.CardId).NotEmpty().WithMessage("卡号ID不能为空");
    }
}

public class DeleteCardNumberCommandHandler : IRequestHandler<DeleteCardNumberCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public DeleteCardNumberCommandHandler(
        ICurrentUserService currentUserService,
        IApplicationDbContext context,
        IPublisher publisher)
    {
        _currentUserService = currentUserService;
        _context = context;
        _publisher = publisher;
    }

    public async Task<Result> Handle(DeleteCardNumberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            return Result.Failure(ErrorMessages.Auth.InvalidUserId);
        var card = await _context.CardNumbers.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);
        if (card is null) return Result.Failure(ErrorMessages.Card.NotFoundCard);
        if (_currentUserService.UserName is null || card.UserId != currentUserId) return Result.Failure(ErrorMessages.Auth.NoPermissionToOperate);
        card.Remove(_currentUserService.UserName);
        await _context.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new CardNumberDeletedEvent(
            card.Id,
            currentUserId,
            card.Cdk,
            _currentUserService.UserName ?? "System"), cancellationToken);
        return Result.Success();
    }
}