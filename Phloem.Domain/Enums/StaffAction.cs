namespace Phloem.Domain.Enums;

/// <summary>What a staff member (or admin) did, as recorded in the staff audit log.</summary>
/// <remarks>
/// Recorded: changes to who can do what, actions on people's accounts, and every time staff open people's messages or
/// personal details. Numbers are stored, so never renumber; add new values at the end.
/// </remarks>
public enum StaffAction
{
    /// <summary>Gave someone a role (Admin, a job role or Courier).</summary>
    RoleGiven = 1,

    /// <summary>Took a role away from someone.</summary>
    RoleRemoved = 2,

    /// <summary>Changed the countries a staff member covers.</summary>
    CountriesChanged = 3,

    /// <summary>Suspended someone's account.</summary>
    UserSuspended = 4,

    /// <summary>Lifted a suspension.</summary>
    UserReinstated = 5,

    /// <summary>Opened the Contact us inbox (people's messages).</summary>
    ContactMessagesViewed = 6,

    /// <summary>Changed a Contact us message's status.</summary>
    ContactMessageUpdated = 7,

    /// <summary>Opened a parcel's details (sender, recipient, address, items).</summary>
    ParcelViewed = 8,

    /// <summary>Changed what a job role is allowed to do (Admin only).</summary>
    RolePermissionsChanged = 9,

    /// <summary>Opened one of a shop's documents (ID, licence, certificate).</summary>
    ShopDocumentOpened = 10,

    /// <summary>Proposed a change that needs a second person.</summary>
    ChangeProposed = 11,

    /// <summary>Approved someone else's change.</summary>
    ChangeApproved = 12,

    /// <summary>Rejected someone else's change.</summary>
    ChangeRejected = 13,

    /// <summary>Took back their own proposal.</summary>
    ChangeWithdrawn = 14,

    /// <summary>Blocked a shop (a second person must confirm).</summary>
    ShopBlocked = 15,

    /// <summary>Unblocked a shop.</summary>
    ShopUnblocked = 16
}
