namespace Legend2Toolbox.Domain.Models;

public class RemoveContentCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string ContentToRemove { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}

