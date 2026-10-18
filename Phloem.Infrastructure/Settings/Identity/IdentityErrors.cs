using Microsoft.AspNetCore.Identity;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>Turns a failed <see cref="IdentityResult"/> into one of Phloem's <see cref="Error"/>s.</summary>
/// <remarks>Shared by AuthService and AccountService, so every Identity failure reaches the app the same way.</remarks>
internal static class IdentityErrors
{
    /// <summary>Identity's codes for "this email / user name is already used".</summary>
    private static readonly HashSet<string> DuplicateCodes =
        [nameof(IdentityErrorDescriber.DuplicateEmail), nameof(IdentityErrorDescriber.DuplicateUserName)];

    /// <summary>
    /// <see cref="AuthErrors.EmailTaken"/> for duplicates; otherwise a validation error carrying Identity's own
    /// messages (e.g. "Passwords must have at least one uppercase ('A'-'Z').").
    /// </summary>
    /// <param name="result">A failed result.</param>
    /// <returns>The error to return to the app.</returns>
    public static Error ToError(IdentityResult result)
    {
        if (result.Errors.Any(e => DuplicateCodes.Contains(e.Code)))
            return AuthErrors.EmailTaken;

        var code = result.Errors.FirstOrDefault()?.Code ?? "Failed";
        var message = string.Join(" ", result.Errors.Select(e => e.Description));

        return Error.Validation($"Auth.{code}",
                                message.Length > 0 ? message : "The account couldn't be updated. Please try again.");
    }
}