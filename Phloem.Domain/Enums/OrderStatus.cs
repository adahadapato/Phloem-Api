namespace Phloem.Domain.Enums;

/// <summary>
/// Where an order is in its life. The normal path is
/// <see cref="AwaitingPayment"/> → <see cref="Paid"/> → <see cref="Preparing"/> →
/// <see cref="OutForDelivery"/> → <see cref="Delivered"/>.
/// </summary>
/// <remarks>
/// The numbers are stored in the database. Never change an existing number once the first
/// migration has run; only add new values at the end.
/// </remarks>
public enum OrderStatus
{
    /// <summary>
    /// Placed but not paid yet. Acts as the customer's saved basket: it never expires and
    /// shows today's prices. Sellers never see orders in this state.
    /// </summary>
    AwaitingPayment = 1,

    /// <summary>Paid. Visible to the seller as a new order to accept. Cancellable for 30 minutes.</summary>
    Paid = 2,

    /// <summary>The seller has accepted the order and is cooking it.</summary>
    Preparing = 3,

    /// <summary>On its way to the customer.</summary>
    OutForDelivery = 4,

    /// <summary>Delivered. Final state.</summary>
    Delivered = 5,

    /// <summary>Cancelled by the customer. Final state. If it had been paid, it needs a refund.</summary>
    Cancelled = 6
}