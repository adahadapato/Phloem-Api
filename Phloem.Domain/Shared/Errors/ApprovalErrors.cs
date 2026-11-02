namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for two-person approval. Nothing here is thrown.</summary>
public static class ApprovalErrors
{
    /// <summary>No such change, or one this person may not see.</summary>
    public static readonly Error NotFound = Error.NotFound(
        "Approval.NotFound", "That change isn't waiting for you.");

    /// <summary>Already approved, rejected, withdrawn or expired.</summary>
    public static readonly Error NoLongerWaiting = Error.Conflict(
        "Approval.NoLongerWaiting", "This change has already been decided or has expired.");

    /// <summary>The proposer tried to approve or reject their own change.</summary>
    public static readonly Error OwnChange = Error.Forbidden(
        "Approval.OwnChange", "You can't approve or reject your own change. Another person has to.");

    /// <summary>The person may not decide this change (e.g. Terms need another owner).</summary>
    public static readonly Error NotAllowed = Error.Forbidden(
        "Approval.NotAllowed", "You can't decide this change.");

    /// <summary>Only the proposer can withdraw a proposal, and only some kinds can be withdrawn.</summary>
    public static readonly Error CannotWithdraw = Error.Forbidden(
        "Approval.CannotWithdraw", "Only the person who proposed it can take it back.");

    /// <summary>Staff only.</summary>
    public static readonly Error StaffOnly = Error.Forbidden(
        "Approval.StaffOnly", "Approvals are for Phloem staff.");
}
