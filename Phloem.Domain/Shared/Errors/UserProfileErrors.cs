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
}