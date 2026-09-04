
namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSetDbData;

public record UpdateScriptSetDbDataCommand(
    Guid Id,
    GameDbTableType TableType,
    string Name,
    string DataJson) : IRequest<Result<Unit>>;

public class UpdateScriptSetDbDataCommandValidator: AbstractValidator<UpdateScriptSetDbDataCommand>
{
    public UpdateScriptSetDbDataCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID不能为空");

        RuleFor(c => c.TableType)
            .NotEmpty().WithMessage("无效的数据表类型");
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("记录名称不能为空")
            .MaximumLength(200).WithMessage("名称过长");
        RuleFor(c => c.DataJson)
            .NotEmpty().WithMessage("数据JSON不能为空")
            .Must(BeValidJson).WithMessage("DataJson必须是有效的JSON格式字符串");
    }
    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class UpdateScriptSetDbDataCommandHandler : IRequestHandler<UpdateScriptSetDbDataCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateScriptSetDbDataCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(UpdateScriptSetDbDataCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Unit>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var entity = await _context.ScriptSetDbDatas
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.ScriptSet!.UserId == currentUserId, cancellationToken);
        if (entity is null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundScriptSetDbData);
        entity.Update(request.TableType, request.Name, request.DataJson);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}