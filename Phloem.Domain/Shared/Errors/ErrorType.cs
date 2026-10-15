namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// What kind of failure an Error is. The API maps it to an HTTP status code.
/// </summary>
public enum ErrorType
{
    Failure = 0,      // 500 / 400: something went wrong
    Validation = 1,   // 400: the input is wrong ("Price must be more than zero")
    NotFound = 2,     // 404: "Dish not found"
    Conflict = 3,     // 409: "That shop name is already taken"
    Forbidden = 4     // 403: "This isn't your shop"
}