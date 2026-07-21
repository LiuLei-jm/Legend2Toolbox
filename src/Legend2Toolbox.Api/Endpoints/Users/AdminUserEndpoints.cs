namespace Legend2Toolbox.Api.Endpoints.Users;

public static class AdminUserEndpoints
{
    public static IEndpointRouteBuilder MapAdminUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/users")
            .WithTags("Admin User Management")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));
        group.MapGet("/", async (
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetUsersQuery(
                pageNumber ?? 1,
                pageSize ?? 10));
            return result.ToMinimalApiResult();
        });
        group.MapPut("/{userId}/lock", async (
            string userId,
            [FromBody] ToggleLockRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ToggleUserLockCommand(userId, request.LockUser);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        group.MapPost("/{userId}/roles", async (string userId,
            [FromBody] AssignRoleRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new AssignRoleCommand(userId, request.RoleName);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        group.MapPut("/{userId}", async (string userId,
            [FromBody] UpdateUserRequest request,
            [FromServices] ISender sender) =>
        {
            var command = request.Adapt<UpdateUserCommand>() with { UserId = userId };
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        group.MapDelete("/{userId}", async (string userId,
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new DeleteUserCommand(userId));
            return result.ToMinimalApiResult();
        });
        return routes;
    }
}
