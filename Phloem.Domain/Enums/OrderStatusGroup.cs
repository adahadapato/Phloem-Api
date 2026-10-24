namespace Phloem.Domain.Enums;

/// <summary>
/// The tabs on "My orders": orders grouped the way buyers think about them, not one tab per
/// <see cref="OrderStatus"/>. Used only to filter lists, never stored.
/// </summary>
/// <remarks>Which statuses each group holds lives in <see cref="OrderStatusGroupExtensions.Statuses"/>.</remarks>
public enum OrderStatusGroup
{
    /// <summary>Every order.</summary>
    All = 0,

    /// <summary>Placed but not paid (<see cref="OrderStatus.AwaitingPayment"/>).</summary>
    WaitingForPayment = 1,

    /// <summary>Paid and not delivered yet: paid, being prepared or on its way.</summary>
    InProgress = 2,

    /// <summary>Delivered.</summary>
    Completed = 3,

    /// <summary>Cancelled (by the buyer, or declined by the shop).</summary>
    Cancelled = 4
}
