using System;

namespace Phloem.Domain.Shared;

/// <summary>
/// Represents the result of an operation, with success/failure status and optional error.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">True if the operation succeeded.</param>
    /// <param name="error">The error, if any.</param>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A success result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error, if any.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a success result (no value).
    /// </summary>
    public static Result Success() => new(true, Error.None);

    public static Result<T> Failure<T>(Error error)
        => Result<T>.Failure(error);

    /// <summary>
    /// Returns a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new Result(false, error);


    /// <summary>
    /// Returns the first failure from the results, or success if all succeed.
    /// </summary>
    public static Result FirstFailureOrSuccess(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Success();
    }
}
