namespace Legend2Toolbox.Domain.Entities.Membership;

public class UserMembership
{
    private UserMembership()
    {
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset ExpireTime { get; private set; }
    public MembershipSource Source { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public DateTimeOffset? LastModifiedOn { get; private set; }

    public static UserMembership Create(Guid userId, int durationInDays, MembershipSource source,
        DateTimeOffset? now = null)
    {
        if (durationInDays <= 0) throw new ArgumentOutOfRangeException(nameof(durationInDays));
        var utcNow = now ?? DateTimeOffset.UtcNow;
        return new UserMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartTime = utcNow,
            ExpireTime = utcNow.AddDays(durationInDays),
            Source = source,
            CreatedOn = utcNow
        };
    }

    public void AdjustDays(int days, DateTimeOffset? now = null)
    {
        if (days == 0) return;
        var utcNow = now ?? DateTimeOffset.UtcNow;
        var extendedExpiry = ExpireTime.AddDays(days);
        ExpireTime = days > 0
            ? (ExpireTime > utcNow ? ExpireTime : utcNow).AddDays(days)
            : (extendedExpiry > utcNow ? extendedExpiry : utcNow);
        LastModifiedOn = utcNow;
    }

    public void Expire(DateTimeOffset? now = null)
    {
        var utcNow = now ?? DateTimeOffset.UtcNow;
        ExpireTime = utcNow;
        LastModifiedOn = utcNow;
    }
}

public enum MembershipSource
{
    RegistrationTrial,
    Payment,
    Admin
}
