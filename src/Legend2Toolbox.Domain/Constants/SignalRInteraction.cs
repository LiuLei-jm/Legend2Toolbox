namespace Legend2Toolbox.Domain.Constants;

public static class SignalRInteraction
{
    public const string Append = "ReceiveWriteCommand";
    public const string Remove = "ReceiveRemoveCommand";
    public const string RemoveList = "ReceiveRemoveListCommand";
    public const string SyncUnexpiredCardsList = "ReceiveSyncUnexpiredCardsListCommand";
}