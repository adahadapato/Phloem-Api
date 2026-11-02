namespace Phloem.Domain.Enums;

/// <summary>Where a change waiting for a second person stands. Numbers are stored: never renumber.</summary>
public enum PendingChangeStatus
{
    /// <summary>Waiting for a second person.</summary>
    Waiting = 1,

    /// <summary>A second person approved it (and it was applied).</summary>
    Approved = 2,

    /// <summary>A second person rejected it (a blocked shop is unblocked).</summary>
    Rejected = 3,

    /// <summary>The person who proposed it took it back, or a newer proposal replaced it.</summary>
    Withdrawn = 4,

    /// <summary>Nobody decided in time (a blocked shop is unblocked).</summary>
    Expired = 5,

    /// <summary>Approved, but applying it failed (the reason is kept); nothing changed.</summary>
    Failed = 6
}
