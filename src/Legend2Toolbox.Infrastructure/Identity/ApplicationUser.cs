using Legend2Toolbox.Domain.Entities.Cards;

namespace Legend2Toolbox.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string NickName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    public virtual ConnectionKey? ConnectionKey { get; set; }
    public virtual ICollection<CardNumber> CardNumbers { get; set; } = [];
    public virtual CardNumberPath? CardNumberPath { get; set; }
}