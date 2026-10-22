using Phloem.Domain.Enums;


namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure a Vendor (shop) can return. Nothing here is thrown.
/// </summary>
public static class VendorErrors
{
    // ---------- The shop ----------

    public static readonly Error NotFound = Error.NotFound(
        "Vendor.NotFound", "That shop wasn't found.");

    public static readonly Error AccountDeleted = Error.Forbidden(
        "Vendor.AccountDeleted", "This account has been deleted.");

    public static readonly Error Blocked = Error.Forbidden(
        "Vendor.Blocked", "This shop is blocked. Please contact Phloem support.");

    /// <summary>Seller setup when the user already owns a shop (one shop per account).</summary>
    public static readonly Error AlreadyHasShop = Error.Conflict(
        "Vendor.AlreadyHasShop", "You already have a shop on Phloem.");

    /// <summary>Seller setup / Shop settings: names are unique (case-insensitive).</summary>
    public static readonly Error NameTaken = Error.Conflict(
        "Vendor.NameTaken", "Another shop already uses that name. Please choose a different one.");

    // ---------- Approval ----------

    /// <summary>PROPOSAL: Reject only makes sense while the shop is waiting for approval.</summary>
    public static readonly Error NotPendingApproval = Error.Conflict(
        "Vendor.NotPendingApproval", "This shop isn't waiting for approval.");

    // ---------- Verification (✓ badge) ----------

    /// <summary>A method, because the message lists which documents are still needed.</summary>
    public static Error MissingDocuments(IEnumerable<VendorDocumentType> missing) => Error.Validation(
        "Vendor.MissingDocuments",
        $"This shop can't be verified yet. Still needed: {string.Join(", ", missing.Select(Describe))}.");

    // ---------- Documents ----------

    public static readonly Error DocumentExpired = Error.Validation(
        "Vendor.DocumentExpired", "This document has already expired. Please upload a current one.");

    public static readonly Error DocumentNotFound = Error.NotFound(
        "Vendor.DocumentNotFound", "That document wasn't found on this shop.");

    public static readonly Error DocumentAlreadyReviewed = Error.Conflict(
        "Vendor.DocumentAlreadyReviewed", "This document has already been reviewed.");

    // ---------- Money ----------

    public static readonly Error InsufficientBalance = Error.Validation(
        "Vendor.InsufficientBalance", "You can't withdraw more than your balance.");

    /// <summary>Readable names for the message ("food hygiene certificate", not "FoodHygieneCertificate").</summary>
    private static string Describe(VendorDocumentType type) => type switch
    {
        VendorDocumentType.BusinessRegistration => "business registration",
        VendorDocumentType.FoodHygieneCertificate => "food hygiene certificate",
        VendorDocumentType.OwnerId => "owner's ID",
        _ => "other document"
    };

    /// <summary>Orders for today have closed (the shop's cut-off has passed): only "Tomorrow" can be chosen.</summary>
    /// <param name="shopName">The shop.</param>
    /// <param name="cutoff">Its cut-off, shop's local time.</param>
    public static Error ClosedForToday(string shopName, TimeOnly cutoff) => Error.Conflict(
        "Vendor.ClosedForToday",
        $"{shopName} takes orders for today until {cutoff:h:mm tt}. Please choose Tomorrow for delivery.");
}
