namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for moderation. Nothing here is thrown.</summary>
public static class ModerationErrors
{
    /// <summary>No such item in the queue, or one in a country the person doesn't cover.</summary>
    public static readonly Error NotFound = Error.NotFound(
        "Moderation.NotFound", "That item isn't waiting in the Moderation queue.");

    /// <summary>Already decided, or the text has changed since.</summary>
    public static readonly Error NoLongerWaiting = Error.Conflict(
        "Moderation.NoLongerWaiting", "This has already been decided, or the seller has changed the text since.");
}
