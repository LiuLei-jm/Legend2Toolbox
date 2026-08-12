using System.ComponentModel.DataAnnotations.Schema;
using Legend2Toolbox.Domain.Common;
using Legend2Toolbox.Domain.Constants;

namespace Legend2Toolbox.Domain.Entities;

public class CardNumberPath : BaseEntity
{
    private CardNumberPath()
    {
    }

    public CardNumberPath(Guid userId)
    {
        BasePath = CardNumberPathInfo.BasePath;
        FileName = CardNumberPathInfo.FileName;
        AllowCustomPath = true;
        UserId = userId;
    }

    public string BasePath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public bool AllowCustomPath { get; private set; } = true;

    [NotMapped] public string FullPath => Path.Combine(BasePath, FileName);

    public Guid UserId { get; private set; }

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