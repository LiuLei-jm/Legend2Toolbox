using FluentValidation;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Application.Common.Models;

namespace Legend2Toolbox.Infrastructure.Files;

public class ClientFileOperationService : IClientFileOperationService
{
    private readonly IAppLogger<ClientFileOperationService> _logger;
    private readonly IValidator<ModifyContentCommand> _validator;

    public ClientFileOperationService(IAppLogger<ClientFileOperationService> logger, IValidator<ModifyContentCommand> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task ModifyFileAppendAsync(ModifyContentCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
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


    public async Task RemoveContentFromFileAsync(ModifyContentCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
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
            if (!originalContentLines.Contains(command.Content))
                return;
            if (originalContentLines.Length == 0 || string.IsNullOrEmpty(command.Content)) return;
            var filteredLines = originalContentLines.Where(line => !line.Contains(command.Content) || string.IsNullOrEmpty(line));
            await File.WriteAllLinesAsync(command.FilePath, filteredLines);
            _logger.LogInfo(command.LogMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"删除内容失败，文件：{command.FilePath}, 错误：{ex.Message}");
        }
    }
}
