namespace Legend2Toolbox.Domain.Models.SignalRCommands;

public class SyncContentListCommand
{
    public string FilePath { get; set; } = string.Empty;
    public List<string> ContentList { get; set; } = [];
}