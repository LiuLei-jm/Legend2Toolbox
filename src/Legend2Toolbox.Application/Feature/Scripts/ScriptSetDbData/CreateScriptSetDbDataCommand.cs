
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Legend2Toolbox.Application.Feature.Scripts.ScriptSetDbData;

public record CreateScriptSetDbDataCommand(Guid ScriptSetId,
                                           GameDbTableType TableType,
                                           string Name,
                                           string DataJson) : IRequest<Result<Guid>>;

public class CreateScriptSetDbDataCommandValidator : AbstractValidator<CreateScriptSetDbDataCommand>
{
    public CreateScriptSetDbDataCommandValidator()
    {
        RuleFor(c => c.ScriptSetId)
            .NotEmpty().WithMessage("脚本套ID不能为空");
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
public class CreateScriptSetDbDataCommandHandler : IRequestHandler<CreateScriptSetDbDataCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateScriptSetDbDataCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateScriptSetDbDataCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId)) return Result<Guid>.Failure(ErrorMessages.AuthError.InvalidUserId);
        var setExists = await _context.ScriptSets
            .AnyAsync(s => s.Id == request.ScriptSetId && s.UserId == currentUserId, cancellationToken);
        if (!setExists) return Result<Guid>.Failure(ErrorMessages.ScriptError.NotFoundScriptSet);
        var entity = Domain.Entities.ScriptSets.ScriptSetDbData.Create(
            request.ScriptSetId,
            request.TableType,
            request.Name,
            request.DataJson);
        _context.ScriptSetDbDatas.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}