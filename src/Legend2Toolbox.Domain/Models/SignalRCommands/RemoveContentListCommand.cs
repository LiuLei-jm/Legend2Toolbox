namespace Legend2Toolbox.Domain.Models.SignalRCommands;

public class RemoveContentListCommand
{
    public string FilePath { get; set; } = string.Empty;
    public List<string> ContentList { get; set; } = [];
}