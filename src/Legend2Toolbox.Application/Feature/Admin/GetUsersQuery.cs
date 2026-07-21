namespace Legend2Toolbox.Application.Feature.Admin;

public record GetUsersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<UserDto>>>;

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