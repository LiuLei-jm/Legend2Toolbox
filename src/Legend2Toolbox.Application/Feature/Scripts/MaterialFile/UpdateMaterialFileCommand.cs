
using Microsoft.AspNetCore.Http;

namespace Legend2Toolbox.Application.Feature.Scripts.MaterialFile;

public record UpdateMaterialFileCommand(Guid Id,
                                        IFormFile? File,
                                        string TargetPath,
                                        string? Password) : IRequest<Result<Unit>>;
public class UpdateMaterialFileCommandValidator : AbstractValidator<UpdateMaterialFileCommand>
{
    public UpdateMaterialFileCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("ID不能为空");
        RuleFor(c => c.TargetPath).NotEmpty().WithMessage("部署放置路径不能为空")
            .MaximumLength(500).WithMessage("路径过长");
        RuleFor(c => c.Password)
        .MaximumLength(100).WithMessage("密码过长");
    }
}
public class UpdateMaterialFileCommandHandler : IRequestHandler<UpdateMaterialFileCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UpdateMaterialFileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<Unit>> Handle(UpdateMaterialFileCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Unit>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var file = await _context.MaterialFiles
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.ScriptSet!.UserId == currentUserId, cancellationToken);
        if (file is null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundMaterialFile);

        var fileName = file.FileName;
        var storagePath = file.StoragePath;
        var fileSize = file.FileSize;

        if(request.File is { Length: > 0 })
        {
            await _fileStorageService.DeleteFileAsync(file.StoragePath, cancellationToken);
            var targetFolder = Path.Combine("materials", _currentUserService.UserId);
            (storagePath, fileSize) = await _fileStorageService.SaveFileAsync(request.File, targetFolder, cancellationToken);
            fileName = request.File.FileName;
        }

        file.Update(fileName, storagePath, request.TargetPath, request.Password ?? string.Empty, fileSize);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}