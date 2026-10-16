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
}
