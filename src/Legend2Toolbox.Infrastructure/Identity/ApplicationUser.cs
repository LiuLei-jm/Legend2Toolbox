namespace Legend2Toolbox.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public virtual SecurityKey? SecurityKey { get; set; }
    public virtual ICollection<CardNumber> CardNumbers { get; set; } = [];
    public virtual CardNumberPath? CardNumberPath { get; set; }
}
