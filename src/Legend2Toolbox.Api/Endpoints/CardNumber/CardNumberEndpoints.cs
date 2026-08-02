using Legend2Toolbox.Application.Feature.CardNumber;

namespace Legend2Toolbox.Api.Endpoints.CardNumber;

public static class CardNumberEndpoints
{
    public static IEndpointRouteBuilder MapCardNumberEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/card").WithTags("Card Number")
            .RequireAuthorization();
        group.MapPost("/create", async (CreateCardNumberRequest req, [FromServices] ISender sender) =>
        {
            var command = req.Adapt<CreateCardNumberCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        group.MapPut("/update/{id:guid}", async (Guid id, [FromBody] UpdateCardNumberRequest req, [FromServices] ISender sender) =>
        {
            var command = req.Adapt<UpdateCardNumberCommand>() with { CardId = id };
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        group.MapDelete("/delete/{id:guid}", async (Guid id, [FromServices] ISender sender) =>
        {
            var command = new DeleteCardNumberCommand(id);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/cards", async ([FromQuery] int? pageNumber,
                                      [FromQuery] int? pageSize,
                                      [FromServices] ISender sender) =>
        {
            var query = new GetCardNumbersQuery(pageNumber ?? 1, pageSize ?? 10);
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
        group.MapGet("/unexpiredcards", async ([FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromServices] ISender sender) =>
        {
            var query = new GetUnexpiredCardNumbersQuery(pageNumber ?? 1, pageSize ?? 10);
        });

        group.MapGet("/path", async ([FromServices] ISender sender) =>
        {
            var query = new GetCardNumberPathQuery();
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
        group.MapPut("/path/update", async ([FromBody] UpdateCardNumberPathRequest request, [FromServices] ISender sender) =>
        {
            var command = request.Adapt<UpdateCardNumberPathCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        return routes;
    }
}
