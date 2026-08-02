namespace Legend2Toolbox.Application.Feature.CardNumber;

public record GetCardNumbersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<CardNumberDto>>>;

public class GetCardNumbersQueryHandler : IRequestHandler<GetCardNumbersQuery, Result<PagedResult<CardNumberDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCardNumbersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<CardNumberDto>>> Handle(GetCardNumbersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<PagedResult<CardNumberDto>>.Failure(ErrorMessages.Auth.InvalidUserId);
        var query = _context.CardNumbers.AsNoTracking().Where(c => c.UserId == currentUserId);
        var totalCount = await query.CountAsync(cancellationToken);
        var cardNumberDtos = await query
            .OrderByDescending(c => c.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<CardNumberDto>()
            .ToListAsync(cancellationToken);
        return Result<PagedResult<CardNumberDto>>.Success(new PagedResult<CardNumberDto>(
            cardNumberDtos, request.PageNumber, request.PageSize, totalCount));
    }
}