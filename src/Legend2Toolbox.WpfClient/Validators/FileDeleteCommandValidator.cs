
namespace Legend2Toolbox.WpfClient.Validators;

public class FileDeleteCommandValidator : AbstractValidator<RemoveContentCommand>
{
    public FileDeleteCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("文件路径不能为空.")
            .Must(PathHelper.IsValidFilePath).WithMessage("无效的文件路径格式.");
        RuleFor(x => x.ContentToRemove)
            .NotEmpty().WithMessage("删除内容不能为空.");

    }
}
