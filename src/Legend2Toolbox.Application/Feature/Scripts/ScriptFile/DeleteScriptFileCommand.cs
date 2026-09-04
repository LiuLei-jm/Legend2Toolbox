
namespace Legend2Toolbox.Application.Feature.Scripts.ScriptFile;

public record DeleteScriptFileCommand(Guid Id) : IRequest<Result<Unit>>;

public class DeleteScriptFileCommandValidator : AbstractValidator<DeleteScriptFileCommand>
{
    public DeleteScriptFileCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("ID不能为空");
    }
}
public class DeleteScriptFileCommandHandler : IRequestHandler<DeleteScriptFileCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteScriptFileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteScriptFileCommand request, CancellationToken cancellationToken)
    {
        var file = await _context.ScriptFiles.FindAsync([request.Id], cancellationToken);
        if (file is null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundScriptFile);
        _context.ScriptFiles.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}