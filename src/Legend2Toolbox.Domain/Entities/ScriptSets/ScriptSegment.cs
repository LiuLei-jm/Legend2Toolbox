using Legend2Toolbox.Domain.Common;

namespace Legend2Toolbox.Domain.Entities.ScriptSets;

public class ScriptSegment : BaseEntity
{
    public Guid ScriptFileId { get; private set; }

    public string TriggerField { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    public ScriptFile? ScriptFile { get; set; }
    private ScriptSegment()
    {

    }
    public static ScriptSegment Create(string triggerField, string content)
    {
        return new ScriptSegment
        {
            Id = Guid.NewGuid(),
            TriggerField = triggerField,
            Content = content
        };
    }
    public void Update(string triggerField, string content)
    {
        TriggerField = triggerField;
        Content = content;
    }
}
