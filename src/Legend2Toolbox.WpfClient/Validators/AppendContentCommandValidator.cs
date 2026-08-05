namespace Legend2Toolbox.WpfClient.Validators;

public class AppendContentCommandValidator : AbstractValidator<AppendContentCommand>
{
    public AppendContentCommandValidator()
    {
        RuleFor(x => x.FilePath)
    .NotEmpty().WithMessage("文件路径不能为空.")
    .Must(PathHelper.IsValidFilePath).WithMessage("无效的文件路径格式.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("写入内容不能为空.");

    }
}
