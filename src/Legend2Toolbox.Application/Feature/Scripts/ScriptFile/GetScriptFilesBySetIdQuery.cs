namespace Legend2Toolbox.Application.Feature.Scripts.ScriptFile;

public record GetScriptFilesBySetIdQuery(Guid ScriptSetId) : IRequest<Result<List<ScriptFileDto>>>;
public class GetScriptFilesBySetIdQueryValidator : AbstractValidator<GetScriptFilesBySetIdQuery>
{
    public GetScriptFilesBySetIdQueryValidator()
    {
        RuleFor(q => q.ScriptSetId).NotEmpty().WithMessage("脚本套ID不能为空");
    }
}
public class GetScriptFilesBySetIdQueryHandler : IRequestHandler<GetScriptFilesBySetIdQuery, Result<List<ScriptFileDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetScriptFilesBySetIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ScriptFileDto>>> Handle(GetScriptFilesBySetIdQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.ScriptFiles
            .AsNoTracking()
            .Where(f => f.ScriptSetId == request.ScriptSetId)
            .ProjectToType<ScriptFileDto>()
            .ToListAsync(cancellationToken);
        return Result<List<ScriptFileDto>>.Success(list);
    }
}