using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.Feature.CardNumber;

public record CardNumberPathResult(string BasePath, string FileName, bool AllowCustomPaths);
public record GetCardNumberPathQuery : IRequest<Result<CardNumberPathResult>>;

public class GetCardNumberPathQueryHandler : IRequestHandler<GetCardNumberPathQuery, Result<CardNumberPathResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCardNumberPathQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CardNumberPathResult>> Handle(GetCardNumberPathQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out Guid userId)) return Result<CardNumberPathResult>.Failure(ErrorMessages.Auth.InvalidUserId);
        var response = await _context.CardNumberPaths.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (response is null) return Result<CardNumberPathResult>.Failure(ErrorMessages.Card.NotFoundCard);
        return Result<CardNumberPathResult>.Success(new CardNumberPathResult(response.BasePath,
                                                                             response.FileName,
                                                                             response.AllowCustomPath));
    }
}