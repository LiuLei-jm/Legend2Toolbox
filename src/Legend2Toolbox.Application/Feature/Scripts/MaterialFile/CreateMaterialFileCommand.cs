
using Microsoft.AspNetCore.Http;

namespace Legend2Toolbox.Application.Feature.Scripts.MaterialFile;

public record CreateMaterialFileCommand(Guid ScriptSetId,
                                        IFormFile File,
                                        string TargetPath,
                                        string Password,
                                        long FileSize) : IRequest<Result<Guid>>;

public class CreateMaterialFileCommandValidator : AbstractValidator<CreateMaterialFileCommand>
{
    public CreateMaterialFileCommandValidator()
    {
        RuleFor(c => c.ScriptSetId)
            .NotEmpty().WithMessage("脚本套ID不能为空");
        RuleFor(c => c.File)
            .NotNull().WithMessage("请选择要上传的素材文件")
            .Must(f => f != null && f.Length > 0).WithMessage("上传的文件不能为空文件");
        RuleFor(c => c.TargetPath)
            .NotEmpty().WithMessage("部署防止路径不能为空")
            .MaximumLength(500).WithMessage("路径过长");
        RuleFor(c => c.Password)
            .MaximumLength(100).WithMessage("密码过长");
    }
}

public class CreateMaterialFileCommandHandler : IRequestHandler<CreateMaterialFileCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public CreateMaterialFileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<Guid>> Handle(CreateMaterialFileCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Guid>.Failure(ErrorMessages.AuthError.InvalidUserId);

        var setExists = await _context.ScriptSets.AnyAsync(s => s.Id == request.ScriptSetId && s.UserId == currentUserId);

        if (!setExists) return Result<Guid>.Failure(ErrorMessages.ScriptError.NotFoundScriptSet);

        var targetFolder = Path.Combine("materials", _currentUserService.UserId);
        var (storagePath, fileSize) = await _fileStorageService.SaveFileAsync(request.File, targetFolder, cancellationToken);

        var materialFile = Domain.Entities.ScriptSets.MaterialFile.Create(
            request.ScriptSetId,
            request.File.FileName,
            storagePath,
            request.TargetPath,
            request.Password,
            fileSize);

        _context.MaterialFiles.Add(materialFile);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(materialFile.Id);
    }
}