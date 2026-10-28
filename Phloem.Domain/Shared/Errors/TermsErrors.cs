namespace Phloem.Domain.Shared.Errors;

/// <summary>Terms acceptance failures. Nothing here is thrown.</summary>
public static class TermsErrors
{
    /// <summary>
    /// The latest Terms haven't been accepted yet: ordering, sending parcels and selling wait until they are.
    /// The website and app look for this code to open "We've updated our Terms".
    /// </summary>
    public static readonly Error NotAccepted = Error.Conflict(
        "Terms.NotAccepted", "We've updated our Terms & Conditions. Please read and accept them to continue.");

    /// <summary>Accepting a version that is no longer the latest (a newer one was published meanwhile).</summary>
    public static readonly Error Outdated = Error.Conflict(
        "Terms.Outdated", "The Terms have just been updated again. Please read the latest version and accept it.");
}
