
namespace Legend2Toolbox.Application.Feature.Scripts.ScriptFile;

public record GetScriptFileByIdQuery(Guid Id) : IRequest<Result<ScriptFileDetailDto>>;
public class GetScriptFileByIdQueryValidator : AbstractValidator<GetScriptFileByIdQuery>
{
    public GetScriptFileByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotEmpty().WithMessage("ID不能为空");
    }
}
public class GetScriptFileByIdQueryHandler : IRequestHandler<GetScriptFileByIdQuery, Result<ScriptFileDetailDto>>
{
    private readonly IApplicationDbContext _context;
    public GetScriptFileByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<ScriptFileDetailDto>> Handle(GetScriptFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await _context.ScriptFiles
            .AsNoTracking()
            .Where(f => f.Id == request.Id)
            .ProjectToType<ScriptFileDetailDto>()
            .FirstOrDefaultAsync(cancellationToken);
        if (file == null) return Result<ScriptFileDetailDto>.Failure(ErrorMessages.ScriptError.NotFoundScriptFile);
        return Result<ScriptFileDetailDto>.Success(file);
    }
}