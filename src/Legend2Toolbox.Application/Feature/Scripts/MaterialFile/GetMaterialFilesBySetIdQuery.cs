
namespace Legend2Toolbox.Application.Feature.Scripts.MaterialFile;

public record GetMaterialFilesBySetIdQuery(Guid ScriptSetId) : IRequest<Result<List<MaterialFileDto>>>;
public class GetMaterialFilesBySetIdQueryValidator: AbstractValidator<GetMaterialFilesBySetIdQuery>
{
    public GetMaterialFilesBySetIdQueryValidator()
    {
        RuleFor(q => q.ScriptSetId).NotEmpty().WithMessage("脚本套ID不能为空");
    }
}
public class GetMaterialFilesBySetIdQueryHandler : IRequestHandler<GetMaterialFilesBySetIdQuery, Result<List<MaterialFileDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMaterialFilesBySetIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MaterialFileDto>>> Handle(GetMaterialFilesBySetIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<List<MaterialFileDto>>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var setExists = await _context.ScriptSets
            .AnyAsync(s => s.Id == request.ScriptSetId && s.UserId == currentUserId, cancellationToken);
       if(!setExists) return Result<List<MaterialFileDto>>.Failure(ErrorMessages.ScriptError.NotFoundScriptSet);
        var list = await _context.MaterialFiles
             .AsNoTracking()
             .Where(m => m.ScriptSetId == request.ScriptSetId)
             .ProjectToType<MaterialFileDto>()
             .ToListAsync(cancellationToken);
        return Result<List<MaterialFileDto>>.Success(list);
    }
}