namespace Phloem.Domain.Shared;

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Gets the value, if the result is a success.
    /// Throws InvalidOperationException if the result is a failure.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The value, if any.</param>
    /// <param name="isSuccess">True if the operation succeeded.</param>
    /// <param name="error">The error, if any.</param>
    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Returns a successful result with the specified value.
    /// </summary>
    public new static Result<T> Success(T value)
        => value is null
            ? Failure(Error.NullValue)  //No <T> needed
            : new Result<T>(value, true, Error.None);

    /// <summary>
    /// Returns a failed result with the specified error.
    /// </summary>
    public new static Result<T> Failure(Error error) =>
        new Result<T>(default!, false, error);

    /// <summary>
    /// Creates a result from a nullable reference type.
    /// </summary>
    //public static Result<T> Create(T value, Error error) where T : class =>
    //    value is null ? Failure<T>(error) : Success(value);

    /// <summary>
    /// Converts a non-generic Result to a generic one (with no value).
    /// </summary>
    //public static Result<T> From(Result result, T? value = default) =>
    //    result.IsSuccess ? Success(value!) : Failure<T>(result.Error);

}
