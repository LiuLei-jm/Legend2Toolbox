namespace Legend2Toolbox.Domain.Models;

public class Result
{
    protected Result(bool isSuccess, string[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string[] Errors { get; }

    public static Result Success()
    {
        return new Result(true, []);
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors.ToArray());
    }

    public static Result Failure(string error)
    {
        return new Result(false, new[] { error });
    }
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, string[] errors) : base(isSuccess, errors)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("失败的结果无法获取 Value");

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, true, Array.Empty<string>());
    }

    public new static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(default, false, errors.ToArray());
    }

    public new static Result<T> Failure(string error)
    {
        return new Result<T>(default, false, new[] { error });
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> mappingFunc)
    {
        if (IsFailure) return Result<TOut>.Failure(Errors.ToArray());
        return Result<TOut>.Success(mappingFunc(Value!));
    }
}