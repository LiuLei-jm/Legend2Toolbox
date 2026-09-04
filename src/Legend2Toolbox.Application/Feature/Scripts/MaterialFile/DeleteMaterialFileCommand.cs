
namespace Legend2Toolbox.Application.Feature.Scripts.MaterialFile;

public record DeleteMaterialFileCommand(Guid Id) : IRequest<Result<Unit>>;
public class DeleteMaterialFileCommandValidator : AbstractValidator<DeleteMaterialFileCommand>
{
    public DeleteMaterialFileCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("ID不能为空");
    }
}
public class DeleteMaterialFileCommandHandler : IRequestHandler<DeleteMaterialFileCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public DeleteMaterialFileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<Unit>> Handle(DeleteMaterialFileCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Unit>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var file = await _context.MaterialFiles
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.ScriptSet!.UserId == currentUserId, cancellationToken);
        if (file is null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundMaterialFile);
        await _fileStorageService.DeleteFileAsync(file.StoragePath, cancellationToken);
        _context.MaterialFiles.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}