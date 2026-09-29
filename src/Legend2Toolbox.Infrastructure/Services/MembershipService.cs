namespace Legend2Toolbox.Infrastructure.Services;

public class MembershipService : IMembershipService
{
    private const int RegistrationDays = 30;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MembershipService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<MembershipStatusDto>> GrantRegistrationMembershipAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var membership = UserMembership.Create(userId, RegistrationDays, MembershipSource.RegistrationTrial);
        _context.UserMemberships.Add(membership);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<MembershipStatusDto>.Failure(ErrorMessages.AuthError.AccountNotExist);
        var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.Member));
        if (!roleResult.Succeeded)
            return Result<MembershipStatusDto>.Failure(roleResult.Errors.Select(x => x.Description).ToArray());
        await _context.SaveChangesAsync(cancellationToken);
        return Result<MembershipStatusDto>.Success(ToDto(membership));
    }

    public async Task<Result<MembershipStatusDto>> AdjustMembershipDaysAsync(string userId, int days,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var parsedUserId))
            return Result<MembershipStatusDto>.Failure(ErrorMessages.AuthError.AccountNotExist);
        if (days == 0) return Result<MembershipStatusDto>.Failure("会员时间调整天数不能为0");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result<MembershipStatusDto>.Failure(ErrorMessages.AuthError.AccountNotExist);
        var membership = await _context.UserMemberships.SingleOrDefaultAsync(x => x.UserId == parsedUserId,
            cancellationToken);
        if (membership is null)
        {
            if (days < 0) return Result<MembershipStatusDto>.Failure("该用户没有有效的会员权益");
            membership = UserMembership.Create(parsedUserId, days, MembershipSource.Admin);
            _context.UserMemberships.Add(membership);
        }
        else
        {
            membership.AdjustDays(days);
        }

        if (membership.ExpireTime > DateTimeOffset.UtcNow)
        {
            if (!await _userManager.IsInRoleAsync(user, nameof(Roles.Member)))
            {
                var addResult = await _userManager.AddToRoleAsync(user, nameof(Roles.Member));
                if (!addResult.Succeeded)
                    return Result<MembershipStatusDto>.Failure(addResult.Errors.Select(x => x.Description).ToArray());
            }
        }
        else if (await _userManager.IsInRoleAsync(user, nameof(Roles.Member)))
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, nameof(Roles.Member));
            if (!removeResult.Succeeded)
                return Result<MembershipStatusDto>.Failure(removeResult.Errors.Select(x => x.Description).ToArray());
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<MembershipStatusDto>.Success(ToDto(membership));
    }

    public async Task<Result<MembershipStatusDto>> GrantPaidMembershipAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<MembershipStatusDto>.Failure(ErrorMessages.AuthError.AccountNotExist);

        var membership = await _context.UserMemberships.SingleOrDefaultAsync(x => x.UserId == userId,
            cancellationToken);
        if (membership is null)
        {
            membership = UserMembership.Create(userId, 30, MembershipSource.Payment);
            _context.UserMemberships.Add(membership);
        }
        else
        {
            membership.AdjustDays(30);
        }

        if (!await _userManager.IsInRoleAsync(user, nameof(Roles.Member)))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.Member));
            if (!roleResult.Succeeded)
                return Result<MembershipStatusDto>.Failure(roleResult.Errors.Select(x => x.Description).ToArray());
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<MembershipStatusDto>.Success(ToDto(membership));
    }

    public async Task<MembershipStatusDto?> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var membership = await _context.UserMemberships.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return membership is null ? null : ToDto(membership);
    }

    public async Task ExpireMembershipsAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var expiredUserIds = await _context.UserMemberships
            .Where(x => x.ExpireTime <= utcNow)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
        foreach (var userId in expiredUserIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is not null && await _userManager.IsInRoleAsync(user, nameof(Roles.Member)))
                await _userManager.RemoveFromRoleAsync(user, nameof(Roles.Member));
        }
    }

    private static MembershipStatusDto ToDto(UserMembership membership) =>
        new(membership.ExpireTime > DateTimeOffset.UtcNow, membership.StartTime, membership.ExpireTime);
}
