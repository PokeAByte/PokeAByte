namespace PokeAByte.Web;

public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public Exception? ExceptionValue { get; }
    public bool IsException => ExceptionValue is not null;

    protected Result(bool isSuccess, Error error, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ExceptionValue = exception;
    }
    public static Result Success() => new Result(true, Error.None);
    public static Result Failure(Error error) => new Result(false, error);
    public static Result Exception(Exception exception) => 
        new Result(false, Error.Exception, exception);
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(true, Error.None, value);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(false, error);

    public static Result<TValue> Exception<TValue>(Exception exception) =>
        new(false, Error.Exception, exception: exception);
    public override string ToString()
    {
        if (IsSuccess)
            return "";
        var errorMessage = $"{Error}\n";
        if (IsException)
            errorMessage += $"Exception Found:\n{ExceptionValue}";
        return errorMessage;
    }
}

public class Result<TValue> : Result
{
    private TValue? _value;
    protected internal Result(bool isSuccess,
        Error error, TValue? resultValue = default,
        Exception? exception = null) :
        base(isSuccess, error, exception)
    {
        _value = resultValue;
    }
    public TValue ResultValue => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result" +
                                              " cannot be accessed.");
    public override string ToString()
    {
        if (!IsSuccess) return base.ToString();
        return _value?.ToString() ?? "";
    }
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}