namespace Legend2Toolbox.Application.Feature.Admin;

public record GetUsersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<UserDto>>>;
public class GetUsersQueryValidator: AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThanOrEqualTo(1).WithMessage("页码必须大于等于1");
        RuleFor(q => q.PageSize).InclusiveBetween(1,100).WithMessage("每页条数必须在1-100之间");
    }
}
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.GetAllUsersAsync(request.PageNumber, request.PageSize);
    }
}