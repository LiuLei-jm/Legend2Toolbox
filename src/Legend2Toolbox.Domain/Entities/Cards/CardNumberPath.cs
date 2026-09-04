using Legend2Toolbox.Domain.Common;
using Legend2Toolbox.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Legend2Toolbox.Domain.Entities.Cards;

public class CardNumberPath : BaseEntity
{
    public string BasePath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public bool AllowCustomPath { get; private set; } = true;
    public Guid UserId { get; private set; }

    private CardNumberPath()
    {
    }

    [NotMapped] public string FullPath => Path.Combine(BasePath, FileName);
    public CardNumberPath(Guid userId)
    {
        BasePath = CardNumberPathInfo.BasePath;
        FileName = CardNumberPathInfo.FileName;
        AllowCustomPath = true;
        UserId = userId;
    }
    public static CardNumberPath Create(Guid userId)
    {
        return new CardNumberPath(userId);
    }

    public void Update(string basePath, string fileName, bool allowCustomPath)
    {
        BasePath = basePath;
        FileName = fileName;
        AllowCustomPath = allowCustomPath;
    }
}