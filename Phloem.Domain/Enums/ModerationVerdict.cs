namespace Phloem.Domain.Enums;

/// <summary>What the AI (or a person) thought of the text.</summary>
public enum ModerationVerdict
{
    /// <summary>Fine.</summary>
    Ok = 1,

    /// <summary>Unclear: a person should look.</summary>
    NeedsLook = 2,

    /// <summary>Breaks the house rules.</summary>
    NotAllowed = 3
}
