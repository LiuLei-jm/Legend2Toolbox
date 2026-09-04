
using Legend2Toolbox.Domain.Common;
using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Domain.Entities.ScriptSets;

public class ScriptSetDbData : BaseEntity
{
    public Guid ScriptSetId { get; private set; }
    public GameDbTableType TableType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string DataJson { get; private set; } = string.Empty;
    public ScriptSet? ScriptSet { get; private set; }

    private ScriptSetDbData() { }
    public static ScriptSetDbData Create(Guid scriptSetId, GameDbTableType tableType, string name, string dataJson)
    {
        return new ScriptSetDbData
        {
            Id = Guid.NewGuid(),
            ScriptSetId = scriptSetId,
            TableType = tableType,
            Name = name,
            DataJson = dataJson
        };
    }
    public void Update(GameDbTableType tableType, string name, string dataJson)
    {
        TableType = tableType;
        Name = name;
        DataJson = dataJson;
    }
}
