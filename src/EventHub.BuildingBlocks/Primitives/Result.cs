namespace BuildingBlocks.Primitives;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public readonly record struct Result
{
    private Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("A successful result cannot have an error message.", nameof(error));
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("A failed result must contain an error message.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string Error { get; }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly record struct Result<T>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, string error)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("A successful result cannot have an error message.", nameof(error));
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("A failed result must contain an error message.", nameof(error));
        }

        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value from a failed result.");

    public static Result<T> Success(T value) => new(true, value, string.Empty);

    public static Result<T> Failure(string error) => new(false, default, error);
}
