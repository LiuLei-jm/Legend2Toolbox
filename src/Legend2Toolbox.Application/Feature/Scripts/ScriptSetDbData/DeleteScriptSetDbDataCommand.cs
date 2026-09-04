
namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSetDbData;

public record DeleteScriptSetDbDataCommand(Guid Id) : IRequest<Result<Unit>>;
public class DeleteScriptSetDbDataCommandValidator : AbstractValidator<DeleteScriptSetDbDataCommand>
{
    public DeleteScriptSetDbDataCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID不能为空");
    }
}

public class DeleteScriptSetDbDataCommandHandler : IRequestHandler<DeleteScriptSetDbDataCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteScriptSetDbDataCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(DeleteScriptSetDbDataCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Unit>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var entity = await _context.ScriptSetDbDatas
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.ScriptSet!.UserId == currentUserId, cancellationToken);
        if (entity is null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundScriptSetDbData);
        _context.ScriptSetDbDatas.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}