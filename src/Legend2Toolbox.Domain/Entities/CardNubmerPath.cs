using Legend2Toolbox.Domain.Common;

namespace Legend2Toolbox.Domain.Entities;

public class CardNumberPath : BaseEntity
{
    public string BasePath { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool AllowCustomPath { get; set; } = true;

    public Guid UserId { get; set; }


}
