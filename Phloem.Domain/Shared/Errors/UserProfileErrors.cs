namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure a UserProfile can return. Nothing here is thrown.
/// </summary>
public static class UserProfileErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "UserProfile.NotFound", "We couldn't find your profile.");

    public static readonly Error AccountDeleted = Error.Forbidden(
        "UserProfile.AccountDeleted", "This account has been deleted.");

    public static readonly Error AddressNotFound = Error.NotFound(
        "UserProfile.AddressNotFound", "That address wasn't found on this account.");

    // ---------- Personal details ----------

    /// <summary>A date of birth in the future, or more than <c>UserProfile.MaximumAge</c> years ago.</summary>
    public static readonly Error InvalidDateOfBirth = Error.Validation(
        "UserProfile.InvalidDateOfBirth", "Please enter your real date of birth.");

    /// <summary>Opening a shop or sending a parcel needs a date of birth and a home address first.</summary>
    public static readonly Error PersonalDetailsRequired = Error.Validation(
        "UserProfile.PersonalDetailsRequired",
        "Please add your date of birth and home address in your profile first.");

    /// <summary>Opening a shop or sending a parcel needs the user to be an adult.</summary>
    public static readonly Error TooYoung = Error.Forbidden(
        "UserProfile.TooYoung", "You must be 18 or older to open a shop or send a parcel.");

    // ---------- Suspension ----------

    /// <summary>An admin tried to suspend their own account.</summary>
    public static readonly Error CannotSuspendYourself = Error.Validation(
        "UserProfile.CannotSuspendYourself", "You can't suspend your own account.");

    /// <summary>Admins can't be suspended (remove the Admin role first).</summary>
    public static readonly Error CannotSuspendAdmin = Error.Forbidden(
        "UserProfile.CannotSuspendAdmin", "Admins can't be suspended.");

    /// <summary>The end date of a suspension must be in the future.</summary>
    public static readonly Error SuspensionEndInPast = Error.Validation(
        "UserProfile.SuspensionEndInPast", "The suspension must end in the future.");

    /// <summary>
    /// Shown when a suspended person tries to sign in (or their session is renewed), e.g.
    /// "Your account is suspended until 28 Oct 2026: repeated fake orders. Contact support if you think this is a
    /// mistake."
    /// </summary>
    /// <param name="untilUtc">When it ends; null = until further notice.</param>
    /// <param name="reason">Why.</param>
    public static Error Suspended(DateTime? untilUtc, string? reason)
    {
        var when = untilUtc is { } until ? $" until {until:d MMM yyyy}" : string.Empty;
        var why = string.IsNullOrWhiteSpace(reason) ? "." : $": {reason.Trim().TrimEnd('.')}.";
        return Error.Forbidden("UserProfile.Suspended",
            $"Your account is suspended{when}{why} Contact support if you think this is a mistake.");
    }
}