
namespace Legend2Toolbox.Api.Exceptions;

public static class ResultExtensions
{
    public static IResult ToMinimalApiResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
        }
        return CreateProblemDetails(result.Errors);
    }

    public static IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        return CreateProblemDetails(result.Errors);
    }

    private static IResult CreateProblemDetails(string[] errors)
    {
        var primaryError = errors.FirstOrDefault() ?? "发生了未知的业务错误.";
        var errorDictionary = new Dictionary<string, string[]>()
        {
            {"DomainErrors", errors }
        };

        return Results.ValidationProblem(
            errors: errorDictionary,
            title: "业务规则处理失败",
            detail: primaryError,
            statusCode: StatusCodes.Status400BadRequest);
    }

}
