using Legend2Toolbox.Shared.Helpers;

namespace Legend2Toolbox.Application.Common.Models;

public class FileDeleteCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string ContentToRemove { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}

public class FileDeleteCommandValidator : AbstractValidator<FileDeleteCommand>
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
