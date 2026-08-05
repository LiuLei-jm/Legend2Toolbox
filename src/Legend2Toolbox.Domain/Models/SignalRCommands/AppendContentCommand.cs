namespace Legend2Toolbox.Domain.Models.SignalRCommands;

public class AppendContentCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

