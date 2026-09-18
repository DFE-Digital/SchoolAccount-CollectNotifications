namespace SchoolAccount.CollectNotifications.Models;

public class Result
{
    public Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result Success() => new(true, null);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    
    public static Result Warning(string warning) => new(true, warning);
    
    public static Result<TValue> Warning<TValue>(TValue? value, string warning) => new Result<TValue>(value, true, warning);

    public static Result Failure(string? error = null) => new(false, error);

    public static Result<TValue> Failure<TValue>(string? error = null) => new(default, false, error);
}

public sealed class Result<TValue> : Result
{
    public Result(TValue? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public TValue Value =>
        IsSuccess
            ? field!
            : throw new InvalidOperationException(
                "The value of a failure result can't be accessed."
            );
}