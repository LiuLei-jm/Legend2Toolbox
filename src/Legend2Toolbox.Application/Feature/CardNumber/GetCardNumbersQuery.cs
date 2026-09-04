namespace Legend2Toolbox.Application.Feature.CardNumber;

public record GetCardNumbersQuery(string? Owner, string? Cdk, string? StartTime, string? EndTime, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<CardNumberDto>>>;
public class GetCardNumbersQueryValidator : AbstractValidator<GetCardNumbersQuery>
{
    public GetCardNumbersQueryValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThanOrEqualTo(1).WithMessage("页码数必须大于等于1");
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100).WithMessage("页面数量必须在1至100之间");
    }
}
public class GetCardNumbersQueryHandler : IRequestHandler<GetCardNumbersQuery, Result<PagedResult<CardNumberDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCardNumbersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<CardNumberDto>>> Handle(GetCardNumbersQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            return Result<PagedResult<CardNumberDto>>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var query = _context.CardNumbers.AsNoTracking().Where(c => c.UserId == currentUserId);

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
            query = query.Where(x => x.EndTime >= parsedStart);
        }
        if (!string.IsNullOrWhiteSpace(request.EndTime) && DateTimeOffset.TryParse(request.EndTime, out var parsedEnd))
        {
            parsedEnd = parsedEnd.AddDays(1);
            query = query.Where(x => x.EndTime < parsedEnd);
        }

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