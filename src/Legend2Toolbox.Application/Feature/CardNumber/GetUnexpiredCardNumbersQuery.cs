namespace Legend2Toolbox.Application.Feature.CardNumber;

public record GetUnexpiredCardNumbersQuery(string? Owner, string? Cdk, string? StartTime, string? EndTime, int PageNumber = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<CardNumberDto>>>;

public class
    GetUnexpiredCardNumbersQueryHandler : IRequestHandler<GetUnexpiredCardNumbersQuery,
    Result<PagedResult<CardNumberDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUnexpiredCardNumbersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<CardNumberDto>>> Handle(GetUnexpiredCardNumbersQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            return Result<PagedResult<CardNumberDto>>.Failure(ErrorMessages.Auth.InvalidUserId);
        var utcNow = DateTimeOffset.UtcNow;
        var query = _context.CardNumbers.AsNoTracking()
            .Where(c => c.UserId == currentUserId && c.EndTime > utcNow);

        if (!string.IsNullOrWhiteSpace(request.Owner))
        {
            query = query.Where(x => x.Owner.Contains(request.Owner));
        }
        if (!string.IsNullOrWhiteSpace(request.Cdk))
        {
            query = query.Where(x => x.Cdk.Contains(request.Cdk));
        }
        if (!string.IsNullOrWhiteSpace(request.StartTime) && DateTimeOffset.TryParse(request.StartTime, out var parsedStart))
        {
            var utcStart = parsedStart.ToUniversalTime();
            query = query.Where(x => x.EndTime >= utcStart);
        }
        if (!string.IsNullOrWhiteSpace(request.EndTime) && DateTimeOffset.TryParse(request.EndTime, out var parsedEnd))
        {
            parsedEnd = parsedEnd.AddDays(1);
            var utcEnd = parsedEnd.ToUniversalTime();
            query = query.Where(x => x.EndTime < utcEnd);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var cardNumberDtos = await query
            .OrderByDescending(c => c.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<CardNumberDto>()
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CardNumberDto>>.Success(new PagedResult<CardNumberDto>(cardNumberDtos,
            request.PageNumber,
            request.PageSize,
            totalCount));
    }
}