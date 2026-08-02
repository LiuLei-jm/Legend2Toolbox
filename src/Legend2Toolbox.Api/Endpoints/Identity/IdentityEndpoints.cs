
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
            if (result.IsFailure) return result.ToMinimalApiResult();
            return Results.SignIn(
                result.Value,
                authenticationScheme: IdentityConstants.BearerScheme);
        }).RequireRateLimiting("login-policy");

        group.MapPost("/change-password", async ([FromBody] ChangePasswordRequest request, [FromServices] ISender sender) =>
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

        group.MapPost("/refresh", async (
             HttpContext httpContext
            ) =>
        {
            var authResult = await httpContext.AuthenticateAsync(IdentityConstants.BearerScheme);
            if (!authResult.Succeeded || authResult.Principal == null)
            {
                throw new UnauthorizedAccessException(ErrorMessages.Auth.TokenExpired);
            }
            var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException(ErrorMessages.Auth.TokenExpired);
            return Results.SignIn(authResult.Principal, authenticationScheme: IdentityConstants.BearerScheme);
        });

        return routes;
    }

}
