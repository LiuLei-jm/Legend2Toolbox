using Legend2Toolbox.Domain.Common;

namespace Legend2Toolbox.Domain.Entities.ScriptSets;

public class ScriptSet : BaseEntity
{
    public string Name { get;private set; } = string.Empty;
    public string? Description { get;private set; }
    public Guid UserId { get; private set; }

    public List<ScriptFile> ScriptFiles { get; set; } = [];
    public List<MaterialFile> MaterialFiles { get; set; } = [];
    public List<ScriptSetDbData> DbDatas { get; set; } = [];

    private ScriptSet()
    {
        
    }
    public static ScriptSet Create(Guid userId,string name, string? description = null)
    {
        return new ScriptSet
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            UserId = userId
        };
    }
    public void Update(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}
