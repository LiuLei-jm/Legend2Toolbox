using Legend2Toolbox.Domain.Common;
using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Domain.Entities.ScriptSets;

public class ScriptFile : BaseEntity
{
    public Guid ScriptSetId { get;private set; }

    public string FileName { get;private set; } = string.Empty;
    public string FilePath { get;private set; } = string.Empty;

    public ScriptFileType Type { get;private set; }

    public string? WholeContent { get;private set; } = string.Empty;

    public List<ScriptSegment> Segments { get; set; } = [];
    public ScriptSet? ScriptSet { get; set; }
    private  ScriptFile()
    {
        
    }
    public static ScriptFile Create(Guid scriptSetId, string fileName, string filePath,ScriptFileType type,string? wholeContent)
    {
        return new ScriptFile
        {
            Id = Guid.NewGuid(),
            ScriptSetId = scriptSetId,
            FileName = fileName,
            FilePath = filePath,
            Type = type,
            WholeContent = type == ScriptFileType.Whole ? wholeContent : null
        };
    }
    public void Update(string fileName, string filePath, ScriptFileType type, string? wholeContent)
    {
        FileName = fileName;
        FilePath = filePath;
        Type = type;
        WholeContent = ScriptFileType.Whole == type ? wholeContent : null;
    }
}
