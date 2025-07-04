using System.Text.Json.Serialization;

namespace NovaPdf.Reporting.Core;

public record ValidationError(string Message, string Property = "Generic");

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public bool IsFailure => !IsSuccess;
    public List<ValidationError> ValidationErrors { get; }

    [JsonConstructor]
    protected Result(bool isSuccess, string error, List<ValidationError>? validationErrors = null)
    {
        if (isSuccess && (!string.IsNullOrEmpty(error) || (validationErrors?.Any() == true)))
            throw new InvalidOperationException("The result can not be a success and have an error.");
        if (!isSuccess && string.IsNullOrEmpty(error) && (validationErrors == null || !validationErrors.Any()))
            throw new InvalidOperationException("The result can not be a failure without specifying errors.");

        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? [];
    }

    public static Result Fail(string error) => new Result(false, error);
    public static Result<T> Fail<T>(string error) where T : class
        => new Result<T>(default, false, error);
    public static Result<T> Fail<T>(Result result) where T : class
        => new Result<T>(default, false, result.Error, result.ValidationErrors);

    public static Result ValidationFailed(IEnumerable<ValidationError> validationErrors)
            => new Result(false, string.Empty, validationErrors.ToList());
    public static Result<T> ValidationFailed<T>(IEnumerable<ValidationError> validationErrors) where T : class
           => new Result<T>(default, false, string.Empty, validationErrors.ToList());

    public static Result Ok()
        => new Result(true, string.Empty);
    public static Result<T> Ok<T>(T value) where T : class
        => new Result<T>(value, true, string.Empty);
}

public class Result<T> : Result where T : class
{
    private readonly T _value;

    public T? Value
    {
        get
        {
            if (!IsSuccess) return default;
            return _value;
        }
    }

    [JsonConstructor]
    protected internal Result(T value, bool isSuccess, string error, List<ValidationError>? validationErrors = null) : base(isSuccess, error, validationErrors)
    {
        _value = value;
    }
}