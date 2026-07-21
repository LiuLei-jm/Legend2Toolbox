namespace Legend2Toolbox.WpfClient.Services.Interfaces;

public interface IClientFileOperationService
{
    Task ModifyFileAppendAsync(FileWriteCommand command);
    Task RemoveContentFromFileAsync(FileDeleteCommand command);
}
