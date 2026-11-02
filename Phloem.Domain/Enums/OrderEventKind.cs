namespace Phloem.Domain.Enums;

/// <summary>One step in an order's timeline. Numbers are stored: never renumber, add at the end.</summary>
/// <remarks>Substitutions, refunds and problem reports join here when they're built.</remarks>
public enum OrderEventKind
{
    /// <summary>The customer placed the order (waiting for payment).</summary>
    Placed = 1,

    /// <summary>Before payment, dishes that had sold out were taken off (the note lists them).</summary>
    ItemsRemoved = 2,

    /// <summary>Payment received.</summary>
    Paid = 3,

    /// <summary>The shop accepted it and started preparing.</summary>
    Accepted = 4,

    /// <summary>The shop declined it (the note gives the reason).</summary>
    Declined = 5,

    /// <summary>The customer cancelled it.</summary>
    Cancelled = 6,

    /// <summary>Cancelled because the shop's owner was suspended.</summary>
    CancelledShopSuspended = 7,

    /// <summary>Sent out with a courier (the note names them).</summary>
    SentOut = 8,

    /// <summary>The courier was changed (the note names the new one).</summary>
    CourierChanged = 9,

    /// <summary>Delivered, with a photo (the note says how it was handed over).</summary>
    Delivered = 10
}
