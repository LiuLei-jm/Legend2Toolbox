using ValidationException = FluentValidation.ValidationException;

namespace Legend2Toolbox.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;


        int statusCode = StatusCodes.Status500InternalServerError;
        string title = "服务器内部错误";
        string clientMessage = "服务器发生意外错误，请联系管理员.";

        switch (exception)
        {
            case BusinessException businessEx:
                statusCode = StatusCodes.Status400BadRequest;
                title = "业务规则错误";
                clientMessage = businessEx.Message;
                _logger.LogWarning("【业务警告】[TraceId: {TraceId}] - {Message}", traceId, businessEx.Message);
                break;

            case UnauthorizedAccessException unauthEx:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "未授权访问";
                clientMessage = unauthEx.Message;
                _logger.LogWarning("【认证警告】[TraceId: {TraceId}] - {Message}", traceId, unauthEx.Message);
                break;

            case DbUpdateConcurrencyException concurrencyEx:
                statusCode = StatusCodes.Status409Conflict;
                title = "数据并发冲突";
                clientMessage = "检测到并发冲突，该数据已被他人修改，请刷新重试.";
                _logger.LogError(concurrencyEx, "【数据库并发冲突】[TraceId: {TraceId}]", traceId);
                break;

            case DbUpdateException dbEx:
                statusCode = StatusCodes.Status400BadRequest;
                title = "数据操作约束";
                clientMessage = AnalyzeDatabaseException(dbEx);
                _logger.LogError(dbEx, "【数据库操作失败】[TraceId: {TraceId}] 内部详细错误: {innerMessage}", traceId, dbEx.InnerException?.Message);
                break;

            case ValidationException valEx:
                statusCode = StatusCodes.Status400BadRequest;
                clientMessage = "输入参数验证失败，请检查表单提示.";
                var validationErrors = valEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
                _logger.LogWarning("【参数校验失败】[TraceId: {TraceId}] - 详情：{@Error}", traceId, validationErrors);

                httpContext.Response.StatusCode = statusCode;
                var validationProblem = new HttpValidationProblemDetails(validationErrors)
                {
                    Title = "输入参数验证失败",
                    Detail = "请检查表单提示以获取详细错误信息",
                    Status = statusCode,
                    Instance = httpContext.Request.Path
                };
                validationProblem.Extensions.Add("traceId", traceId);
                httpContext.Response.StatusCode = statusCode;
                await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
                return true;

            default:
                _logger.LogCritical(exception, "【系统致命崩溃】[TraceId: {TraceId}]", traceId);
                break;
        }
        ;

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = clientMessage,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions.Add("traceId", traceId);
        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private string AnalyzeDatabaseException(DbUpdateException dbEx)
    {
        if (dbEx.InnerException == null) return "数据保存失败，请检测输入合规性.";
        var innerMessage = dbEx.InnerException.Message;
        if (innerMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            return "关联的数据项不存在或正被占用（外键限制）。";

        if (innerMessage.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase))
            return "核心条目已存在，请勿重复提交。";
        return "数据校验未通过，请检查表单数据。";
    }
}
