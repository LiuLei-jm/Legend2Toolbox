using Legend2Toolbox.Application.Feature.CardNumber;

namespace Legend2Toolbox.Api.Endpoints.CardNumber;

public static class CardNumberEndpoints
{
    public static IEndpointRouteBuilder MapCardNumberEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/card").WithTags("Card Number")
            .RequireAuthorization();
        group.MapGet("/path", async ([FromServices] ISender sender) =>
        {
            var query = new GetCardNumberPathQuery();
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
        group.MapPut("/path/update", async ([FromBody] UpdateCardNumberPathRequest request,[FromServices] ISender sender) =>
        {
            var command = request.Adapt<UpdateCardNumberPathCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        return routes;
    }
}
