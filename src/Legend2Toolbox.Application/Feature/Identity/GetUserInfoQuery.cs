using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.Feature.Identity;

public record GetUserInfoQuery : IRequest<Result<UserInfoDto>>;
public class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuery, Result<UserInfoDto>>
{
    private readonly IIdentityService _identityService;
    public GetUserInfoQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<Result<UserInfoDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.GetUserInfoAsync(request);
    }
}
