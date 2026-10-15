using Phloem.Domain.Enums;
using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Domain.Errors;

/// <summary>
/// Every business-rule failure a Vendor (shop) can return. Nothing here is thrown.
/// </summary>
public static class VendorErrors
{
    // ---------- The shop ----------

    public static readonly Error AccountDeleted = Error.Forbidden(
        "Vendor.AccountDeleted", "This account has been deleted.");

    public static readonly Error Blocked = Error.Forbidden(
        "Vendor.Blocked", "This shop is blocked. Please contact Phloem support.");

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
}