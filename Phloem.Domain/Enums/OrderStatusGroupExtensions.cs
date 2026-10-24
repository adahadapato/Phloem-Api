namespace Phloem.Domain.Enums;

/// <summary>Which <see cref="OrderStatus"/> values belong to each <see cref="OrderStatusGroup"/>.</summary>
public static class OrderStatusGroupExtensions
{
    /// <summary>The statuses in a group; null for <see cref="OrderStatusGroup.All"/> (no filter).</summary>
    /// <param name="group">The tab.</param>
    /// <returns>The statuses to keep, or null to keep every order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A group without a mapping (add it here).</exception>
    public static IReadOnlyCollection<OrderStatus>? Statuses(this OrderStatusGroup group) => group switch
    {
        OrderStatusGroup.All               => null,
        OrderStatusGroup.WaitingForPayment => [OrderStatus.AwaitingPayment],
        OrderStatusGroup.InProgress        => [OrderStatus.Paid, OrderStatus.Preparing, OrderStatus.OutForDelivery],
        OrderStatusGroup.Completed         => [OrderStatus.Delivered],
        OrderStatusGroup.Cancelled         => [OrderStatus.Cancelled],
        _ => throw new ArgumentOutOfRangeException(nameof(group))
    };
}
