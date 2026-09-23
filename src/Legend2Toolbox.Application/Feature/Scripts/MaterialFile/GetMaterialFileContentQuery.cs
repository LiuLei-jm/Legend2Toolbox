using System.Security.Cryptography;

namespace Legend2Toolbox.Application.Feature.Scripts.MaterialFile;

public record GetMaterialFileContentQuery(Guid Id) : IRequest<Result<MaterialFileContentDto>>;

public record MaterialFileContentDto(
    Stream Content,
    string FileName,
    string Sha256);

public class GetMaterialFileContentQueryValidator : AbstractValidator<GetMaterialFileContentQuery>
{
    public GetMaterialFileContentQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage("素材文件ID不能为空");
    }
}

public class GetMaterialFileContentQueryHandler
    : IRequestHandler<GetMaterialFileContentQuery, Result<MaterialFileContentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public GetMaterialFileContentQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<MaterialFileContentDto>> Handle(
        GetMaterialFileContentQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
        {
            return Result<MaterialFileContentDto>.Failure(ErrorMessages.AuthError.InvalidUserId);
        }

        var materialFile = await _context.MaterialFiles
            .FirstOrDefaultAsync(
                m => m.Id == request.Id && m.ScriptSet!.UserId == currentUserId,
                cancellationToken);

        if (materialFile is null)
        {
            return Result<MaterialFileContentDto>.Failure(ErrorMessages.ScriptError.NotFoundMaterialFile);
        }

        Stream content;
        try
        {
            content = await _fileStorageService.OpenReadAsync(materialFile.StoragePath, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            return Result<MaterialFileContentDto>.Failure(ErrorMessages.ScriptError.NotFoundMaterialFile);
        }
        catch (DirectoryNotFoundException)
        {
            return Result<MaterialFileContentDto>.Failure(ErrorMessages.ScriptError.NotFoundMaterialFile);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(materialFile.Sha256))
            {
                var hash = await SHA256.HashDataAsync(content, cancellationToken);
                materialFile.SetSha256(Convert.ToHexString(hash).ToLowerInvariant());
                await _context.SaveChangesAsync(cancellationToken);
                content.Position = 0;
            }

            return Result<MaterialFileContentDto>.Success(new MaterialFileContentDto(
                content,
                materialFile.FileName,
                materialFile.Sha256));
        }
        catch
        {
            await content.DisposeAsync();
            throw;
        }
    }
}
