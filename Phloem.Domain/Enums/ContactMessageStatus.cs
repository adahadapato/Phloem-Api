namespace Phloem.Domain.Enums;

/// <summary>Where a Contact us message is in the team's inbox.</summary>
public enum ContactMessageStatus
{
    /// <summary>Not looked at yet (counted on the admin dashboard).</summary>
    New = 1,

    /// <summary>Someone has replied (by email) and may be waiting for an answer back.</summary>
    Replied = 2,

    /// <summary>Done: nothing more to do.</summary>
    Closed = 3
}
