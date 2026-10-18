using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Presentation.Extensions;

/// <summary>
/// Turns a handler's <see cref="Result"/> into an HTTP response, so endpoints stay one line.
/// </summary>
/// <remarks>
/// <para><b>Success:</b> 200 with the value, or 204 when there's nothing to return.</para>
/// <para><b>Failure:</b> ProblemDetails, the same shape as <c>GlobalExceptionHandler</c> (with <c>traceId</c>), plus a
/// <c>code</c> field (e.g. "Auth.InvalidCredentials") that the app can switch on.</para>
/// </remarks>
internal static class ResultExtensions
{
    /// <summary>204 No Content, or the error.</summary>
    /// <param name="result">The handler's result.</param>
    public static IResult ToHttp(this Result result) =>
        result.IsSuccess ? Results.NoContent() : result.Error.ToProblem();

    /// <summary>200 OK with the value, or the error.</summary>
    /// <typeparam name="T">The value's type.</typeparam>
    /// <param name="result">The handler's result.</param>
    public static IResult ToHttp<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();

    /// <summary>An error as ProblemDetails with the matching status code.</summary>
    /// <param name="error">The error.</param>
    public static IResult ToProblem(this Error error)
    {
        var (status, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Invalid request"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Not allowed"),
            _ => (StatusCodes.Status500InternalServerError, "Something went wrong")
        };

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: error.Message,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}