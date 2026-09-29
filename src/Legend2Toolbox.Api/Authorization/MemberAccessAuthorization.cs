using Microsoft.AspNetCore.Authorization;
using Legend2Toolbox.Domain.Enums;
using System.Security.Claims;

namespace Legend2Toolbox.Api.Authorization;

public sealed class MemberAccessRequirement : IAuthorizationRequirement
{
}

public sealed class MemberAccessAuthorizationHandler : AuthorizationHandler<MemberAccessRequirement>
{
    private readonly ApplicationDbContext _context;

    public MemberAccessAuthorizationHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MemberAccessRequirement requirement)
    {
        if (context.User.IsInRole(nameof(Roles.SuperAdmin)))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) return;

        var utcNow = DateTimeOffset.UtcNow;
        var isActiveMember = await (
            from user in _context.Users
            join userRole in _context.UserRoles on user.Id equals userRole.UserId
            join role in _context.Roles on userRole.RoleId equals role.Id
            join membership in _context.UserMemberships on user.Id equals membership.UserId
            where user.Id == userId
                  && user.IsActive
                  && !user.IsDeleted
                  && role.Name == nameof(Roles.Member)
                  && membership.ExpireTime > utcNow
            select user.Id
        ).AnyAsync();
        if (isActiveMember) context.Succeed(requirement);
    }
}
