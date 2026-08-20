namespace Legend2Toolbox.Domain.Models.SignalRCommands;

public class RemoveContentCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}