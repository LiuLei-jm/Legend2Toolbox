namespace Legend2Toolbox.WpfClient.Services;

public class ClientFileOperationService : IClientFileOperationService
{
    private readonly IAppLogger<ClientFileOperationService> _logger;
    private readonly IValidator<AppendContentCommand> _fileWriteValidator;
    private readonly IValidator<RemoveContentCommand> _fileDeleteValidator;
    private readonly IValidator<RemoveContentListCommand> _fileDeleteListValidator;
    private readonly IValidator<SyncContentListCommand> _fileSyncListValidator;


    private readonly SemaphoreSlim _fileLock = new(1, 1);
    public ClientFileOperationService(IAppLogger<ClientFileOperationService> logger, IValidator<AppendContentCommand> fileWriteValidator, IValidator<RemoveContentCommand> fileDeleteValidator, IValidator<RemoveContentListCommand> fileDeleteListValidator, IValidator<SyncContentListCommand> fileSyncListValidator)
    {
        _logger = logger;
        _fileWriteValidator = fileWriteValidator;
        _fileDeleteValidator = fileDeleteValidator;
        _fileDeleteListValidator = fileDeleteListValidator;
        _fileSyncListValidator = fileSyncListValidator;
    }

    public async Task AppendContentAsync(AppendContentCommand command)
    {
        var validationResult = await _fileWriteValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            _logger.LogError($"文件写入命令校验失败：{string.Join(", ", validationResult.Errors)}");
            return;
        }
        await _fileLock.WaitAsync();
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
            var originalContent = await File.ReadAllLinesAsync(command.FilePath);
            var contentToAppend = command.Content.Trim();

            if (originalContent.Any(line => line.Trim() == contentToAppend))
            {
                _logger.LogInfo($"已存在 {contentToAppend} 跳过写入.");
                return;
            }

            await File.AppendAllTextAsync(command.FilePath, contentToAppend + Environment.NewLine);
            _logger.LogInfo($"新增卡号：{contentToAppend}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"修改文件失败, 文件:{command.FilePath}, 错误:{ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }


    public async Task RemoveContentAsync(RemoveContentCommand command)
    {
        var validationResult = await _fileDeleteValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            _logger.LogError($"文件删除命令校验失败：{string.Join(", ", validationResult.Errors)}");
            return;
        }
        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(command.FilePath))
            {
                _logger.LogError($"文件不存在，无法删除内容：{command.FilePath}");
                return;
            }
            var originalContentLines = await File.ReadAllLinesAsync(command.FilePath);
            if (originalContentLines.Length == 0) return;

            var targetContent = command.ContentToRemove.Trim();
            if (!originalContentLines.Any(line => line.Trim() == targetContent))
                return;

            var filteredLines = originalContentLines.Where(line => line.Trim() != targetContent).ToList();
            await File.WriteAllLinesAsync(command.FilePath, filteredLines);
            _logger.LogInfo($"删除卡号：{targetContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"删除内容失败，文件：{command.FilePath}, 错误：{ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task RemoveContentListAsync(RemoveContentListCommand command)
    {
        var validationResult = await _fileDeleteListValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            _logger.LogError($"文件批量删除命令校验失败：{string.Join(", ", validationResult.Errors)}");
            return;
        }
        if (command.ContentList == null || command.ContentList.Count == 0) return;

        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(command.FilePath))
            {
                _logger.LogError($"文件不存在，无法删除内容：{command.FilePath}");
                return;
            }
            var originalContentLines = await File.ReadAllLinesAsync(command.FilePath);
            if (originalContentLines.Length == 0) return;

            var targetSet = new HashSet<string>(command.ContentList.Select(c => c.Trim()));
            List<string> filteredLines = [];
            bool isFileChanged = false;

            foreach (var line in originalContentLines)
            {
                var lineTrimmed = line.Trim();
                if (targetSet.Contains(lineTrimmed))
                {
                    _logger.LogInfo($"删除卡号: {lineTrimmed}");
                    isFileChanged = true;
                }
                else
                {
                    filteredLines.Add(line);
                }
            }
            if (isFileChanged)
            {
                await File.WriteAllLinesAsync(command.FilePath, filteredLines);
                _logger.LogInfo("批量删除卡号完成.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"批量删除内容失败，文件：{command.FilePath}, 错误：{ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SyncUnexpiredCardsListAsync(SyncContentListCommand command)
    {
        var validationResult = await _fileSyncListValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            _logger.LogError($"文件同步命令校验失败：{string.Join(", ", validationResult.Errors)}");
            return;
        }
        if (command.ContentList == null || command.ContentList.Count == 0) return;

        await _fileLock.WaitAsync();
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
                await File.WriteAllLinesAsync(command.FilePath, command.ContentList.Select(c => c.Trim()));
                _logger.LogError($"文件不存在，已自动创建并同步：{command.ContentList.Count} 条卡号.");
                return;
            }
            var originalContentLines = await File.ReadAllLinesAsync(command.FilePath);
            if (originalContentLines.Length == 0) return;

            var targetSet = new HashSet<string>(command.ContentList.Select(c => c.Trim()));
            List<string> filteredLines = [];
            bool isFileChanged = false;

            foreach (var line in originalContentLines)
            {
                var lineTrimmed = line.Trim();
                if (!targetSet.Contains(lineTrimmed))
                {
                    _logger.LogInfo($"删除卡号：{lineTrimmed}");
                    isFileChanged = true;
                }
                else
                {
                    filteredLines.Add(line);
                }
            }

            var existingSet = new HashSet<string>(originalContentLines.Select(l => l.Trim()));

            foreach (var line in targetSet)
            {
                var lineTrimmed = line.Trim();
                if (!existingSet.Contains(lineTrimmed))
                {
                    _logger.LogInfo($"新增卡号：{lineTrimmed}");
                    filteredLines.Add(line);
                    isFileChanged = true;
                }
            }
            if (isFileChanged)
            {
                await File.WriteAllLinesAsync(command.FilePath, filteredLines);
                _logger.LogInfo("批量同步卡号完成.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"批量同步内容失败，文件：{command.FilePath}, 错误：{ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
