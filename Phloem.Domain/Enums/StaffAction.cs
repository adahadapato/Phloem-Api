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
    ShopUnblocked = 16,

    /// <summary>Opened an order's details (customer, address, phone, timeline).</summary>
    OrderViewed = 17,

    /// <summary>Saved a new wording for an AI prompt.</summary>
    AiPromptSaved = 18,

    /// <summary>Brought back an older wording of an AI prompt.</summary>
    AiPromptRestored = 19,

    /// <summary>Put an AI prompt back to Phloem's default wording.</summary>
    AiPromptReset = 20,

    /// <summary>Opened one AI call from the log (it can hold people's text).</summary>
    AiCallViewed = 21,

    /// <summary>Changed a page's search engine text (SEO) by hand.</summary>
    SeoEdited = 22,

    /// <summary>Asked the AI to write a page's search engine text again.</summary>
    SeoRewritten = 23,

    /// <summary>Approved text in the Moderation queue (a dish becomes visible).</summary>
    ContentApproved = 24,

    /// <summary>Rejected text in the Moderation queue (the seller sees the reason).</summary>
    ContentRejected = 25
}
