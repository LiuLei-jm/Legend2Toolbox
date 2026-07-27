using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.Feature.CardNumber;

public record UpdateCardNumberPathCommand(string BasePath, string FileName, bool AllowCustomPath) : IRequest<Result>;
public class UpdateCardNumberPathCommandValidator : AbstractValidator<UpdateCardNumberPathCommand>
{
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    public UpdateCardNumberPathCommandValidator()
    {
        RuleFor(x => x.BasePath).NotEmpty().WithMessage("基础路径不能为空")
            .Must(NotContainInvalidPathChars).WithMessage("基础路径包含无效字符");
        RuleFor(x => x.FileName).NotEmpty().WithMessage("文件路径不能为空")
            .Must(NotContainInvalidFileNameChars).WithMessage("卡号文件路径包含无效字符");
    }

    private bool NotContainInvalidPathChars(string path)
    {
        if (string.IsNullOrEmpty(path)) return true;
        return !path.Any(c => InvalidPathChars.Contains(c));
    }

    private bool NotContainInvalidFileNameChars(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return true;
        return !fileName.Any(c => InvalidFileNameChars.Contains(c));
    }
}
public class UpdateCardNumberPathCommandHandler : IRequestHandler<UpdateCardNumberPathCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCardNumberPathCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateCardNumberPathCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out Guid userId)) return Result.Failure(ErrorMessages.Auth.InvalidUserId);
        var cardNumberPath = await _context.CardNumberPaths.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (cardNumberPath is null) return Result.Failure(ErrorMessages.Card.NotFoundCard);
        cardNumberPath.Update(request.BasePath, request.FileName, request.AllowCustomPath);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}