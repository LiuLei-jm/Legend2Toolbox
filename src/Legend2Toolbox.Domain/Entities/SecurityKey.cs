using Legend2Toolbox.Domain.Common;
using System.Security.Cryptography;

namespace Legend2Toolbox.Domain.Entities;

public class SecurityKey : AuditableEntity
{
    public string Key { get; private set; } = string.Empty;
    public Guid UserId { get; set; }
    private SecurityKey() { }
    public SecurityKey(Guid userId, string? user)
    {
        Key = GenerateSecurityKey();
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = user;
        UserId = userId;
    }

    public static SecurityKey Create(Guid userId, string? user)
    {
        return new SecurityKey(
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
