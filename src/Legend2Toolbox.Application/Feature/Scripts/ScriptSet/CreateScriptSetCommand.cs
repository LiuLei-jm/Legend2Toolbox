namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSet;

public record CreateScriptSetCommand(
    string Name,
    string? Description
    ) : IRequest<Result<Guid>>;

public class CreateScriptSetCommandValidator : AbstractValidator<CreateScriptSetCommand>
{
    public CreateScriptSetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("脚本套名称不能为空")
            .MaximumLength(200).WithMessage("名称过长");
    }
}

public class CreateScriptSetCommandHandler : IRequestHandler<CreateScriptSetCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateScriptSetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateScriptSetCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Guid>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var entity = Domain.Entities.ScriptSets.ScriptSet.Create(currentUserId, request.Name, request.Description);

        await _context.ScriptSets.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}