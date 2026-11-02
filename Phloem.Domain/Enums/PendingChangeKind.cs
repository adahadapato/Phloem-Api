namespace Phloem.Domain.Enums;

/// <summary>What a change waiting for a second person is. Numbers are stored: never renumber, add at the end.</summary>
/// <remarks>Refunds and seller withdrawals are added here when payments are built.</remarks>
public enum PendingChangeKind
{
    /// <summary>Publish a new Terms or Privacy Policy version. Applied only once another owner approves.</summary>
    PublishLegalDocument = 1,

    /// <summary>
    /// A shop was blocked at once (it may be urgent); a second person must confirm within the confirmation window,
    /// otherwise it is unblocked automatically.
    /// </summary>
    ConfirmShopBlock = 2
}
