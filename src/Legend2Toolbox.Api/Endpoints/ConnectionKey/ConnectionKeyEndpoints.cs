namespace Legend2Toolbox.Api.Endpoints.ConnectionKey;

public static class ConnectionKeyEndpoints
{
    public static IEndpointRouteBuilder MapConnectionKeyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/security-key")
            .WithTags("Security key").RequireAuthorization();
        group.MapPost("/", async ([FromServices] ISender sender) =>
        {
            var command = new GenerateKeyCommand();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        group.MapGet("/", async ([FromServices] ISender sender) =>
        {
            var query = new GetKeyQuery();
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
        group.MapGet("/clients", async ([FromServices] ISender sender) =>
        {
            var query = new GetConnectionClientsQuery();
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
        return routes;
    }
}