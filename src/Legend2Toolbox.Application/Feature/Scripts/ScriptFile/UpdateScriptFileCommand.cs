namespace Legend2Toolbox.Application.Feature.Scripts.ScriptFile;

public record UpdateScriptFileCommand(Guid Id,
    string FileName,
    string FilePath,
    ScriptFileType Type,
    string? WholeContent,
    List<ScriptSegmentDto>? Segments) : IRequest<Result<Unit>>;

public class UpdateScriptFileCommandValidator : AbstractValidator<UpdateScriptFileCommand>
{
    public UpdateScriptFileCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("ID不能为空");
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
public class UpdateScriptFileCommandHandler : IRequestHandler<UpdateScriptFileCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateScriptFileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateScriptFileCommand request, CancellationToken cancellationToken)
    {
        var file = await _context.ScriptFiles.Include(f => f.Segments)
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (file == null) return Result<Unit>.Failure(ErrorMessages.ScriptError.NotFoundScriptFile);

        file.Update(request.FileName, request.FilePath, request.Type, request.WholeContent);

        if (file.Type == ScriptFileType.Partial && request.Segments?.Count > 0)
        {
            foreach (var reqSeg in request.Segments)
            {
                var existingSeg = reqSeg.Id.HasValue && reqSeg.Id.Value != Guid.Empty
                    ? file.Segments.FirstOrDefault(s => s.Id == reqSeg.Id.Value)
                    : null;
                if (existingSeg != null)
                {
                    existingSeg.Update(reqSeg.TriggerField, reqSeg.Content);
                }
                else
                {
                    file.Segments.Add(Domain.Entities.ScriptSets.ScriptSegment.Create(reqSeg.TriggerField, reqSeg.Content));
                }
            }
            var requestIds = request.Segments.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();
            var toRemove = file.Segments
                .Where(s => !requestIds.Contains(s.Id) && !request.Segments.Any(r => r.TriggerField == s.TriggerField)).ToList();
            foreach (var removeSeg in toRemove)
            {
                _context.ScriptSegments.Remove(removeSeg);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}