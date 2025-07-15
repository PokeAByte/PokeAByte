namespace PokeAByte.Web;

public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public Exception? ExceptionValue { get; }
    public bool IsException => ExceptionValue is not null;
    public string AdditionalErrorMessage { get; }

    protected Result(bool isSuccess, Error error, Exception? exception = null, string additionalErrorMessage = "")
    {
        IsSuccess = isSuccess;
        Error = error;
        ExceptionValue = exception;
        AdditionalErrorMessage = additionalErrorMessage;
    }

    public static Result Success() => new Result(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(true, Error.None, value);

    public static Result Failure(Error error, string additionalErrorMessage = "") =>
        new Result(false, error, additionalErrorMessage: additionalErrorMessage);

    public static Result<TValue> Failure<TValue>(Error error, string additionalErrorMessage = "") =>
        new(false, error, additionalErrorMessage: additionalErrorMessage);

    public static Result Exception(Exception exception) =>
        new Result(false, Error.Exception, exception);

    public override string ToString()
    {
        if (IsSuccess)
            return "";
        var errorMessage = $"{Error}\n";
        if (IsException)
            errorMessage += $"Exception Found:\n{ExceptionValue}\n";
        if (!string.IsNullOrEmpty(AdditionalErrorMessage))
            errorMessage += $"Message: {AdditionalErrorMessage}";
        return errorMessage;
    }
}

public class Result<TValue> : Result
{
    private TValue? _value;
    protected internal Result(bool isSuccess,
        Error error, TValue? resultValue = default,
        Exception? exception = null,
        string additionalErrorMessage = "") :
        base(isSuccess, error, exception, additionalErrorMessage)
    {
        _value = resultValue;
    }

    public TValue? ResultValue => IsSuccess
        ? _value!
        : default;

    public override string ToString()
    {
        if (!IsSuccess) return base.ToString();
        return _value?.ToString() ?? "";
    }

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}