using Legend2Toolbox.Application.Common.Models;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IClientFileOperationService
{
    Task ModifyFileAppendAsync(ModifyContentCommand command);
    Task RemoveContentFromFileAsync(ModifyContentCommand command);
}
