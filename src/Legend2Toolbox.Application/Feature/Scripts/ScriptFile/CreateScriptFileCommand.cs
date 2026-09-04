
namespace Legend2Toolbox.Application.Feature.Scripts.ScriptFile;

public record CreateScriptFileCommand(
    Guid ScriptSetId,
    string FileName,
    string FilePath,
    ScriptFileType Type,
    string? WholeContent,
    List<ScriptSegmentDto>? Segments) : IRequest<Result<Guid>>;

public class CreateScriptFileCommandValidator : AbstractValidator<CreateScriptFileCommand>
{
    public CreateScriptFileCommandValidator()
    {
        RuleFor(c => c.ScriptSetId).NotEmpty().WithMessage("所属脚本套ID不能为空");
        RuleFor(c => c.FileName).NotEmpty().WithMessage("文件名不能为空")
            .MaximumLength(200).WithMessage("名称过长");
        RuleFor(c => c.FilePath).NotEmpty().WithMessage("文件路径不能为空")
            .MaximumLength(500).WithMessage("路径过长");
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.WholeContent)
            .NotEmpty()
            .When(c => c.Type == ScriptFileType.Whole)
            .WithMessage("完整替换类型的脚本必须填写 WholeContent 内容");
        RuleFor(c => c.Segments)
            .NotEmpty()
            .When(c => c.Type == ScriptFileType.Partial)
            .WithMessage("部分节点追加类型的脚本必须提供至少一个 Trigger 追加片段");

        When(c => c.Type == ScriptFileType.Partial && c.Segments != null, () =>
        {
            RuleForEach(c => c.Segments).ChildRules(segment =>
            {
                segment.RuleFor(s => s.TriggerField).NotEmpty().WithMessage("触发标识(TriggerField)不能为空");
                segment.RuleFor(s => s.Content).NotEmpty().WithMessage("追加脚本内容(Content)不能为空");
            });
        });
    }
}

public class CreateScriptFileCommandHandler : IRequestHandler<CreateScriptFileCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateScriptFileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateScriptFileCommand request, CancellationToken cancellationToken)
    {
        var setExists = await _context.ScriptSets.AnyAsync(s => s.Id == request.ScriptSetId, cancellationToken);
        if (!setExists) return Result<Guid>.Failure(ErrorMessages.ScriptError.NotFoundScriptSet);
        var scriptFile = Domain.Entities.ScriptSets.ScriptFile.Create(request.ScriptSetId, request.FileName, request.FilePath, request.Type, request.WholeContent);
        if (scriptFile.Type == ScriptFileType.Partial && request.Segments?.Count > 0)
        {
            foreach(var s in request.Segments)
            {
                scriptFile.Segments.Add(Domain.Entities.ScriptSets.ScriptSegment.Create(s.TriggerField, s.Content));
            }
        }
        _context.ScriptFiles.Add(scriptFile);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(scriptFile.Id);
    }
}