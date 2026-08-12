namespace Legend2Toolbox.Api.Endpoints.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapCustomIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Authorization");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] ISender sender) =>
        {
            var command = request.Adapt<RegisterCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        }).RequireRateLimiting("register-policy");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] ISender sender
        ) =>
        {
            var query = request.Adapt<LoginCommand>();
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        }).RequireRateLimiting("login-policy");

        group.MapGet("/userinfo", async ([FromServices] ISender sender) =>
        {
            var query = new GetUserInfoQuery();
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        }).RequireAuthorization();

        group.MapPost("/change-password",
            async ([FromBody] ChangePasswordRequest request, [FromServices] ISender sender) =>
            {
                var command = request.Adapt<ChangePasswordCommand>();
                var result = await sender.Send(command);
                return result.ToMinimalApiResult();
            }).RequireAuthorization();

        group.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, ISender sender) =>
        {
            var command = request.Adapt<ForgotPasswordCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, ISender sender) =>
        {
            var command = request.Adapt<ResetPasswordCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request,
            [FromServices] ISender sender
        ) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });

        group.MapPut("/update", async ([FromBody] UpdateUserProfileRequest request,
            ISender sender) =>
        {
            var command = request.Adapt<UpdateUserProfileCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        }).RequireAuthorization();

        return routes;
    }
}