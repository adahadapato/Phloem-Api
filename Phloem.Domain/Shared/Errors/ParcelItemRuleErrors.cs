namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for the "what can be sent" list. Nothing here is thrown.</summary>
public static class ParcelItemRuleErrors
{
    /// <summary>No line with this id (it may have been deleted already).</summary>
    public static readonly Error NotFound = Error.NotFound(
        "ParcelItemRule.NotFound", "This item isn't on the list any more.");

    /// <summary>A reorder that doesn't list every line exactly once.</summary>
    public static readonly Error OrderMismatch = Error.Conflict(
        "ParcelItemRule.OrderMismatch", "The list has changed since you opened it. Please reload and try again.");
}
