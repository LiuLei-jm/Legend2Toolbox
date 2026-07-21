namespace Legend2Toolbox.WpfClient.Services;

public class ClientFileOperationService : IClientFileOperationService
{
    private readonly IAppLogger<ClientFileOperationService> _logger;
    private readonly IValidator<FileWriteCommand> _fileWriteValidator;
    private readonly IValidator<FileDeleteCommand> _fileDeleteValidator;

    public ClientFileOperationService(IAppLogger<ClientFileOperationService> logger, IValidator<FileWriteCommand> fileWriteValidator, IValidator<FileDeleteCommand> fileDeleteValidator)
    {
        _logger = logger;
        _fileWriteValidator = fileWriteValidator;
        _fileDeleteValidator = fileDeleteValidator;
    }

    public async Task ModifyFileAppendAsync(FileWriteCommand command)
    {
        var validationResult = await _fileWriteValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            string errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogDebug($"参数校验失败:{errors}");
            return;
        }
        try
        {
            string? directoryPath = Path.GetDirectoryName(command.FilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInfo($"创建目录：{directoryPath}");
            }
            if (!File.Exists(command.FilePath))
            {
                await File.WriteAllTextAsync(command.FilePath, string.Empty);
                _logger.LogInfo($"创建文件: {command.FilePath}");
            }
            string originalContent = await File.ReadAllTextAsync(command.FilePath);
            if (originalContent.Contains(command.Content))
            {
                _logger.LogInfo($"已存在 {command.Content.Trim()} 跳过写入.");
                return;
            }
            await File.AppendAllTextAsync(command.FilePath, command.Content);
            _logger.LogInfo(command.LogMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"修改文件失败, 文件:{command.FilePath}, 错误:{ex.Message}");
        }
    }


    public async Task RemoveContentFromFileAsync(FileDeleteCommand command)
    {
        var validationResult = await _fileDeleteValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            string errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogDebug($"参数校验失败:{errors}");
            return;
        }
        try
        {
            if (!File.Exists(command.FilePath))
            {
                _logger.LogError($"文件不存在，无法删除内容：{command.FilePath}");
                return;
            }
            var originalContentLines = await File.ReadAllLinesAsync(command.FilePath);
            if (!originalContentLines.Contains(command.ContentToRemove))
                return;
            if (originalContentLines.Length == 0 || string.IsNullOrEmpty(command.ContentToRemove)) return;
            var filteredLines = originalContentLines.Where(line => !line.Contains(command.ContentToRemove) || string.IsNullOrEmpty(line));
            await File.WriteAllLinesAsync(command.FilePath, filteredLines);
            _logger.LogInfo(command.LogMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"删除内容失败，文件：{command.FilePath}, 错误：{ex.Message}");
        }
    }
}
