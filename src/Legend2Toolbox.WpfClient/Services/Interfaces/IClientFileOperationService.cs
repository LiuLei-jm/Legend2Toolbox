namespace Legend2Toolbox.WpfClient.Services.Interfaces;

public interface IClientFileOperationService
{
    Task AppendContentAsync(AppendContentCommand command);
    Task RemoveContentAsync(RemoveContentCommand command);
    Task RemoveContentListAsync(RemoveContentListCommand command);
    Task SyncUnexpiredCardsListAsync(SyncContentListCommand command);
}
