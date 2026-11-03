namespace Phloem.Domain.Enums;

/// <summary>Whether a dish's text has passed the check (buyers only see <see cref="Approved"/> dishes).</summary>
public enum ContentReviewStatus
{
    /// <summary>Checked and fine (also every dish that existed before checks began).</summary>
    Approved = 1,

    /// <summary>New or changed text, being checked by the AI (usually under a minute).</summary>
    Checking = 2,

    /// <summary>The AI flagged it (or couldn't check): a person decides.</summary>
    WithStaff = 3,

    /// <summary>Not allowed: the seller sees why and can edit it (which checks it again).</summary>
    Rejected = 4
}
