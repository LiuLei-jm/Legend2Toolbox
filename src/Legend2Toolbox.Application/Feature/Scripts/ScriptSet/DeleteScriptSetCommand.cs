namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSet;

public record DeleteScriptSetCommand(Guid Id) : IRequest<Result<Unit>>;

public class DeleteScriptSetCommandValidator : AbstractValidator<DeleteScriptSetCommand>
{
    public DeleteScriptSetCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("ID不能为空");
    }
}
public class DeleteScriptSetCommandHandler : IRequestHandler<DeleteScriptSetCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteScriptSetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserSerivce)
    {
        _context = context;
        _currentUserService = currentUserSerivce;
    }

    public async Task<Result<Unit>> Handle(DeleteScriptSetCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Unit>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var entity = await _context.ScriptSets.FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == currentUserId, cancellationToken);
        if (entity is null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundScriptSet);
        _context.ScriptSets.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}