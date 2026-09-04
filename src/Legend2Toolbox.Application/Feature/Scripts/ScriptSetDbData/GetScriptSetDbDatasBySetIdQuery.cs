
namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSetDbData;

public record GetScriptSetDbDatasBySetIdQuery(
    Guid ScriptSetId,
    GameDbTableType? TableType = null
    ) : IRequest<Result<List<ScriptSetDbDataDto>>>;

public class GetScriptSetDbDatasBySetIdQueryValidator : AbstractValidator<GetScriptSetDbDatasBySetIdQuery>
{
    public GetScriptSetDbDatasBySetIdQueryValidator()
    {
        RuleFor(q => q.ScriptSetId)
            .NotEmpty().WithMessage("脚本套ID不能为空");
        When(q => q.TableType.HasValue, () =>
        {
            RuleFor(q => q.TableType!.Value).IsInEnum().WithMessage("无效的数据类型");
        });
    }
}
public class GetScriptSetDbDatasBySetIdQueryHandler : IRequestHandler<GetScriptSetDbDatasBySetIdQuery, Result<List<ScriptSetDbDataDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetScriptSetDbDatasBySetIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ScriptSetDbDataDto>>> Handle(GetScriptSetDbDatasBySetIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<List<ScriptSetDbDataDto>>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var setExists = await _context.ScriptSets
            .AnyAsync(s => s.Id == request.ScriptSetId && s.UserId == currentUserId, cancellationToken);
        if (!setExists) return Result<List<ScriptSetDbDataDto>>.Failure(ErrorMessages.ScriptError.NotFoundScriptSetDbData);
        var query = _context.ScriptSetDbDatas
            .AsNoTracking()
            .Where(d => d.ScriptSetId == request.ScriptSetId);

        if (request.TableType.HasValue)
        {
            query = query.Where(d => d.TableType == request.TableType.Value);
        }

        var list = await query
            .ProjectToType<ScriptSetDbDataDto>()
            .ToListAsync(cancellationToken);
        return Result<List<ScriptSetDbDataDto>>.Success(list);
    }
}