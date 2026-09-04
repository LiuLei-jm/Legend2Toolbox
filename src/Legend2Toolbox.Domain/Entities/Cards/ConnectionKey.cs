using Legend2Toolbox.Domain.Common;
using System.Security.Cryptography;

namespace Legend2Toolbox.Domain.Entities.Cards;

public class ConnectionKey : AuditableEntity
{
    public string Key { get; private set; } = string.Empty;
    public Guid UserId { get; set; }

    private ConnectionKey()
    {
    }

    public ConnectionKey(Guid userId, string? user)
    {
        Key = GenerateSecurityKey();
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = user;
        UserId = userId;
    }
    public static ConnectionKey Create(Guid userId, string? user)
    {
        return new ConnectionKey(
            userId,
            user
        );
    }

    public void RegenerateKey(string user)
    {
        Key = GenerateSecurityKey();
        LastModifiedBy = user;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    private string GenerateSecurityKey()
    {
        var bytes = new byte[512];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}