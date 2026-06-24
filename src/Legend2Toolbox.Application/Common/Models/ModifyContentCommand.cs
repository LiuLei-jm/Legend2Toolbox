using FluentValidation;
using Legend2Toolbox.Shared.Helpers;

namespace Legend2Toolbox.Application.Common.Models;

public class ModifyContentCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}

public class ModifyContentCommandValidator : AbstractValidator<ModifyContentCommand>
{
    public ModifyContentCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("文件路径不能为空.")
            .Must(PathHelper.IsValidFilePath).WithMessage("无效的文件路径格式.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("写入内容不能为空.");
    }
}
