namespace Legend2Toolbox.WpfClient.Validators;

public class SyncContentListCommandValidator : AbstractValidator<SyncContentListCommand>
{
    public SyncContentListCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("文件路径不能为空")
            .Must(PathHelper.IsValidFilePath).WithMessage("无效的文件路径格式");
        RuleFor(x => x.ContentList)
            .NotNull().WithMessage("同步列表不能为空");
    }
}
