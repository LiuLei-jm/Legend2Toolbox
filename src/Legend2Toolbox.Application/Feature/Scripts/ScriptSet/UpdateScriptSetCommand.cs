namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSet;

public record UpdateScriptSetCommand(Guid Id, string Name, string? Description) : IRequest<Result<Unit>>;
public class UpdateScriptSetCommandValidator : AbstractValidator<UpdateScriptSetCommand>
{
    public UpdateScriptSetCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("ID不能为空");
        RuleFor(s => s.Name).NotEmpty().WithMessage("脚本套名称不能为空")
            .MaximumLength(200).WithMessage("名称过长");
    }
}
public class UpdateScriptSetCommandHandler : IRequestHandler<UpdateScriptSetCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateScriptSetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(UpdateScriptSetCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Unit>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var entity = await _context.ScriptSets.FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == currentUserId, cancellationToken);
        if (entity is null)
        {
            return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundScriptSet);
        }
        entity.Update(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}