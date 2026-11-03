namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for order chats. Nothing here is thrown.</summary>
public static class ChatErrors
{
    /// <summary>No such order, or one the person isn't part of (or not paid yet).</summary>
    public static readonly Error NotFound = Error.NotFound(
        "Chat.NotFound", "There's no chat for this order.");

    /// <summary>The chat is read-only now.</summary>
    public static readonly Error Closed = Error.Conflict(
        "Chat.Closed", $"This chat closed {Entities.OrderChat.OpenDaysAfterOrder} days after the order ended. " +
                       "If something is wrong, use Contact us.");

    /// <summary>No such message, or not yours.</summary>
    public static readonly Error MessageNotFound = Error.NotFound(
        "Chat.MessageNotFound", "That message isn't yours, or doesn't exist.");

    /// <summary>Already reported and waiting for staff.</summary>
    public static readonly Error AlreadyReported = Error.Conflict(
        "Chat.AlreadyReported", "This chat has already been sent to Phloem's team. We'll be in touch.");
}
