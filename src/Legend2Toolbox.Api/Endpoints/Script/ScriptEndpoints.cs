using Legend2Toolbox.Application.Feature.Scripts.MaterialFile;
using Legend2Toolbox.Application.Feature.Scripts.DTOS;
using Legend2Toolbox.Application.Feature.Scripts.ScriptFile;
using Legend2Toolbox.Application.Feature.Scripts.ScriptSet;
using Legend2Toolbox.Application.Feature.Scripts.ScriptSetDbData;

namespace Legend2Toolbox.Api.Endpoints.Script;

public static class ScriptEndpoints
{
    public static IEndpointRouteBuilder MapScriptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scripts")
            .RequireAuthorization();

        MapScriptSetEndpoints(group);
        MapScriptFileEndpoints(group);
        MapMaterialFileEndpoints(group);
        MapScriptSetDbDataEndpoints(group);

        return app;
    }

    private static void MapScriptSetEndpoints(RouteGroupBuilder group)
    {
        var setGroup = group.MapGroup("/sets").WithTags("Script Set");
        setGroup.MapPost("/", async ([FromBody] CreateScriptSetRequest req, ISender sender) =>
        {
            var command = req.Adapt<CreateScriptSetCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        setGroup.MapPut("/{id:guid}", async (Guid id,
                                             [FromBody] UpdateScriptSetRequest req,
                                             [FromServices] ISender sender) =>
        {
            var command = req.Adapt<UpdateScriptSetCommand>() with { Id = id };
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        setGroup.MapDelete("{id:guid}", async (
            Guid id,
           [FromServices] ISender sender
            ) =>
        {
            var command = new DeleteScriptSetCommand(id);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        setGroup.MapGet("/", async (
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromServices] ISender sender = default!
            ) =>
        {
            var query = new GetScriptSetsQuery(pageNumber, pageSize);
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
    }
    private static void MapScriptFileEndpoints(RouteGroupBuilder group)
    {
        var fileGroup = group.MapGroup("/files").WithTags("Script File");
        fileGroup.MapPost("/", async (
            [FromBody] CreateScriptFileRequest req,
            [FromServices] ISender sender
            ) =>
        {
            var command = req.Adapt<CreateScriptFileCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        fileGroup.MapPut("/{id:guid}", async (Guid id,
            [FromBody] UpdateScriptFileRequest req,
            [FromServices] ISender sender
            ) =>
        {
            var command = req.Adapt<UpdateScriptFileCommand>() with { Id = id };
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        fileGroup.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender
            ) =>
        {
            var command = new DeleteScriptFileCommand(id);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        fileGroup.MapGet("/by-set/{scriptSetId:guid}", async (
            Guid scriptSetId,
            [FromServices] ISender sender
            ) =>
        {
            var query = new GetScriptFilesBySetIdQuery(scriptSetId);
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        });
        fileGroup.MapGet("/{id:guid}", async (Guid id,
            [FromServices] ISender sender) => {
                var query = new GetScriptFileByIdQuery(id);
                var result = await sender.Send(query);
                return result.ToMinimalApiResult();
            });
    }

    private static void MapMaterialFileEndpoints(RouteGroupBuilder group)
    {
        var materialGroup = group.MapGroup("/material").WithTags("Material File");
        materialGroup.MapPost("/", async (
            [FromForm] Guid scriptSetId,
            IFormFile file,
            [FromForm] string targetPath,
            [FromForm] string? password,
            [FromForm] long? fileSize,
            [FromServices] ISender sender
            ) =>
        {
            if (file is null) return Results.BadRequest(Result<Guid>.Failure("上传的文件不能为空"));
            var command = new CreateMaterialFileCommand(
                scriptSetId,
                file,
                targetPath,
                password ?? string.Empty,
                fileSize ?? file.Length
                );
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        }).DisableAntiforgery();
        materialGroup.MapPut("/{id:guid}", async (
            Guid id,
            IFormFile? file,
            [FromForm] string targetPath,
            [FromForm] string? password,
            [FromServices] ISender sender
            ) =>
        {
            var command = new UpdateMaterialFileCommand(
                id,
                file,
                targetPath,
                password ?? string.Empty
                );
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        }).DisableAntiforgery();
        materialGroup.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender
            ) =>
        {
            var command = new DeleteMaterialFileCommand(id);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        materialGroup.MapGet("/by-set/{scriptSetId:guid}", async (
            Guid scriptSetId,
            [FromServices] ISender sender
            ) =>
        {
            var query = new GetMaterialFilesBySetIdQuery(scriptSetId);
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        })
            .Produces<List<MaterialFileDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        materialGroup.MapGet("/{id:guid}/content", async (
            Guid id,
            HttpContext httpContext,
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetMaterialFileContentQuery(id));
            if (result.IsFailure) return Results.NotFound();

            var file = result.Value;
            httpContext.Response.Headers["X-Checksum-SHA256"] = file.Sha256;
            return Results.File(
                file.Content,
                "application/octet-stream",
                file.FileName,
                enableRangeProcessing: true);
        })
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Headers["X-Checksum-SHA256"] = new OpenApiHeader
                {
                    Description = "素材文件上传时保存的 SHA-256，小写十六进制格式",
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Pattern = "^[a-f0-9]{64}$"
                    }
                };
                return operation;
            });
    }

    private static void MapScriptSetDbDataEndpoints(RouteGroupBuilder group)
    {
        var dbDataGroup = group.MapGroup("/db-datas").WithTags("Script Set DB Data");
        dbDataGroup.MapPost("/", async (
            [FromBody] CreateScriptSetDbDataRequest req,
            [FromServices] ISender sender
            ) =>
        {
            var command = req.Adapt<CreateScriptSetDbDataCommand>();
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        dbDataGroup.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateScriptSetDbDataRequest req,
            [FromServices] ISender sender
            ) =>
        {
            var command = req.Adapt<UpdateScriptSetDbDataCommand>() with { Id = id };
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        dbDataGroup.MapDelete("/{id:guid}", async (Guid id,
            [FromServices] ISender sender) =>
        {
            var command = new DeleteScriptSetDbDataCommand(id);
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        });
        dbDataGroup.MapGet("/by-set/{scriptSetId:guid}", async (Guid scriptSetId,
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetScriptSetDbDatasBySetIdQuery(scriptSetId));
            return result.ToMinimalApiResult();
        });
    }

}
