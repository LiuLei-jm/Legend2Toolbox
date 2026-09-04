namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSet;

public record GetScriptSetsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<ScriptSetDto>>>;
public class GetScriptSetsQueryValidator: AbstractValidator<GetScriptSetsQuery>
{
    public GetScriptSetsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("页码必须大于等于1");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("每页条数必须在1-100之间");
    }
}
public class GetScriptSetsQueryHandler : IRequestHandler<GetScriptSetsQuery, Result<PagedResult<ScriptSetDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public GetScriptSetsQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<Result<PagedResult<ScriptSetDto>>> Handle(GetScriptSetsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<PagedResult<ScriptSetDto>>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var query = _context.ScriptSets.AsNoTracking().Where(s => s.UserId == currentUserId);
        var totalCount = await query.CountAsync(cancellationToken);
        var scriptSetDtos = await query
            .OrderByDescending(s => s.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<ScriptSetDto>()
            .ToListAsync(cancellationToken);
        return Result<PagedResult<ScriptSetDto>>.Success(new PagedResult<ScriptSetDto>(
            scriptSetDtos, request.PageNumber, request.PageSize, totalCount
            )
        );
    }
}